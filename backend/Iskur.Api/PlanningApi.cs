using System.Text.Json;
using static Iskur.Api.PlanningRules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public partial class ApiController
{
    async Task<PlanWindow?> Window(int id, int year, int month) =>
        await db.Set<PlanWindow>().FindAsync(await ParticipationFaculty(id), year, month);

    async Task<DateOnly[]> Exams(int? planId) =>
        await db.Set<PlanExamDate>().Where(x => x.MonthlyPlanId == planId)
            .Select(x => x.Date).OrderBy(x => x).ToArrayAsync();

    async Task<object> PlanView(int id, int year, int month, MonthlyPlan? plan) => new
    {
        id = plan?.Id, status = plan?.Status ?? "Draft", updatedAt = plan?.UpdatedAt,
        version = plan is null ? null : Convert.ToBase64String(plan.Version),
        window = await Window(id, year, month),
        examDates = await Exams(plan?.Id),
        days = (plan?.Days ?? []).OrderBy(x => x.Date).Select(x => new
        { x.Id, x.Date, x.Kind, x.Minutes, x.Note, x.Conflict, x.Decision, x.Reason, x.DecidedAt })
    };

    async Task<bool> EditingAllowed(int id, int year, int month, DateOnly today)
    {
        var window = await Window(id, year, month);
        return window is null ? today <= new DateOnly(year, month, 1).AddMonths(1).AddDays(-1)
            : today >= window.Opens && today <= window.Closes;
    }

    async Task<bool> HasRecordedActivity(IEnumerable<int> ids)
    {
        var keys = ids.ToArray();
        return await db.Set<SlotAttendance>().AnyAsync(x => keys.Contains(x.PlanDayId)) ||
            await db.Set<LeaveSlot>().AnyAsync(x => keys.Contains(x.PlanDayId)) ||
            await db.Set<AbsenceReport>().AnyAsync(x => keys.Contains(x.PlanDayId));
    }

    async Task Snapshot(MonthlyPlan plan, string action)
    {
        db.Add(new PlanRevision
        {
            MonthlyPlanId = plan.Id, Action = action, ActorId = Uid, RecordedAt = Now,
            Snapshot = JsonSerializer.Serialize(new
            {
                plan.Status, examDates = await Exams(plan.Id),
                days = plan.Days.OrderBy(x => x.Date).Select(x => new
                { x.Date, x.Kind, x.Minutes, x.Note, x.Conflict, x.Decision, x.Reason, x.DecidedById, x.DecidedAt })
            })
        });
    }

    [HttpGet("students/{id:int}/plans/{year:int}/{month:int}/history")]
    public async Task<IActionResult> PlanHistory(int id, int year, int month)
    {
        if (!await Can(id)) return Forbid();
        return Ok(await db.Set<PlanRevision>().Where(x => x.MonthlyPlan.ParticipationId == id &&
            x.MonthlyPlan.Year == year && x.MonthlyPlan.Month == month).OrderByDescending(x => x.Id)
            .Select(x => new { x.Action, x.RecordedAt, x.Snapshot }).ToListAsync());
    }

    [HttpPut("students/{id:int}/plans/{year:int}/{month:int}"), Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> SavePlan(int id, int year, int month, PlanDto input)
    {
        if (!await Can(id)) return Forbid();
        if (year is < 2020 or > 2100 || month is < 1 or > 12 || input.Days is null ||
            input.Days.Count > 31 || input.ExamDates?.Count > 31)
            return Invalid("Geçersiz ay veya gün listesi.");

        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var today = FinalRules.LocalDate(Now);
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        var participation = await db.Participations.SingleAsync(x => x.Id == id);
        if (!participation.Active) return Invalid("Program katılımınız pasif.");
        if (!await EditingAllowed(id, year, month, today))
            return Invalid("Bu ay için plan düzenleme aralığı kapalı.");

        var plan = await db.Plans.Include(x => x.Days).SingleOrDefaultAsync(x =>
            x.ParticipationId == id && x.Year == year && x.Month == month);
        if (plan?.Status == "Submitted") return Changed("Değişiklik için planı geri çekin.");
        if (plan is not null && Convert.ToBase64String(plan.Version) != input.Version)
            return Changed("Plan değişti. Yeniden yükleyin.");

        var dates = input.Days.Select(x => x.Date).ToHashSet();
        var exams = (input.ExamDates ?? []).ToHashSet();
        bool ProgramWeekday(DateOnly date) => date >= participation.Start && date <= participation.End &&
            date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
        bool Eligible(DateOnly date) => date >= first && date <= last && ProgramWeekday(date);

        if (dates.Count != input.Days.Count || dates.Any(d => !Eligible(d)) || exams.Any(d => !Eligible(d)) ||
            dates.Overlaps(exams))
            return Invalid("Çalışma ve sınav tarihleri farklı, hafta içi ve katılım dönemine ait olmalıdır.");

        var lastWeek = WeekStart(last);
        var lastWeekEnd = lastWeek.AddDays(6);
        var holidays = await db.Holidays.Where(x => x.Date >= first && x.Date <= lastWeekEnd)
            .ToDictionaryAsync(x => x.Date);
        bool IsWorkday(DateOnly date) => ProgramWeekday(date) &&
            (!holidays.TryGetValue(date, out var holiday) || holiday.AfternoonOnly);
        var courses = await db.Courses.Where(x => x.ParticipationId == id).ToListAsync();
        var neighbors = await db.PlanDays.Where(x => x.MonthlyPlan.ParticipationId == id &&
            x.Decision != "Rejected" && x.Date >= first.AddDays(-6) && x.Date <= last.AddDays(6) &&
            (x.Date < first || x.Date > last)).Select(x => new { x.Date, x.MonthlyPlan.Status }).ToListAsync();

        if (ExceedsWeeklyLimit(dates.Concat(neighbors.Select(x => x.Date))))
            return Invalid("Bir haftada en fazla 3 çalışma tarihi seçilebilir. Komşu ay da hesaba katılır.");

        if (input.Submit)
        {
            var selectedWeeks = dates.Concat(neighbors.Where(x => x.Status == "Submitted").Select(x => x.Date))
                .Select(WeekStart).ToHashSet();
            var nextMonth = last.AddDays(1);
            var canScheduleNextMonth = Enumerable.Range(1, lastWeekEnd.DayNumber - last.DayNumber)
                .Select(last.AddDays).Any(IsWorkday);
            if (canScheduleNextMonth)
            {
                var nextWindow = await Window(id, nextMonth.Year, nextMonth.Month);
                canScheduleNextMonth = nextWindow is null || today <= nextWindow.Closes;
            }

            var missingWeeks = Enumerable.Range(0, last.DayNumber - first.DayNumber + 1)
                .Select(first.AddDays).Where(date => IsWorkday(date) && !exams.Contains(date))
                .Select(WeekStart).Distinct()
                .Where(week => !selectedWeeks.Contains(week) && (week != lastWeek || !canScheduleNextMonth))
                .Select(week => $"{week:dd.MM.yyyy}–{week.AddDays(6):dd.MM.yyyy}").ToArray();
            if (missingWeeks.Length > 0)
                return Invalid($"{string.Join(", ", missingWeeks)} haftalarında en az bir çalışma günü seçin.");
        }

        var planned = new List<PlanDay>();
        foreach (var day in input.Days)
        {
            if (day.Kind is not ("Full" or "Morning" or "Afternoon") || day.Note?.Length > 500)
                return Invalid("Çalışma türünü ve en fazla 500 karakterlik açıklamayı kontrol edin.");
            if (holidays.TryGetValue(day.Date, out var holiday) && (!holiday.AfternoonOnly || day.Kind != "Morning"))
                return Invalid($"{day.Date:dd.MM.yyyy}: Resmî tatil seçilemez.");

            var conflict = courses.Any(c => c.DayOfWeek == (int)day.Date.DayOfWeek &&
                Conflicts(c.StartMinute, c.EndMinute, day.Kind));
            if (input.Submit && conflict && string.IsNullOrWhiteSpace(day.Note))
                return Invalid($"{day.Date:dd.MM.yyyy}: Ders çakışması için açıklama girin.");
            planned.Add(new PlanDay { Date = day.Date, Kind = day.Kind, Note = day.Note?.Trim() ?? "",
                Conflict = conflict, Minutes = FinalRules.Slots(day.Kind).Sum(FinalRules.Minutes) });
        }

        if (plan is null)
        {
            plan = new MonthlyPlan { ParticipationId = id, Year = year, Month = month };
            db.Add(plan);
        }
        else
        {
            await Snapshot(plan, input.Submit ? "BeforeSubmit" : "BeforeSave");
            db.PlanDays.RemoveRange(plan.Days);
            var previousExams = await db.Set<PlanExamDate>().Where(x => x.MonthlyPlanId == plan.Id).ToListAsync();
            db.RemoveRange(previousExams.Where(x => !exams.Contains(x.Date)));
            exams.ExceptWith(previousExams.Select(x => x.Date));
        }

        foreach (var date in exams)
            db.Add(new PlanExamDate { MonthlyPlan = plan, Date = date });
        plan.Days = planned;
        plan.Status = input.Submit ? "Submitted" : "Draft";
        plan.UpdatedAt = Now;
        Log(input.Submit ? "PlanSubmit" : "PlanSave", id);
        await db.SaveChangesAsync();
        var view = await PlanView(id, year, month, plan);
        await transaction.CommitAsync();
        return Ok(view);
    }

    [HttpPost("students/{id:int}/plans/{year:int}/{month:int}/withdraw"), Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> Withdraw(int id, int year, int month, [FromBody] string version)
    {
        if (!await Can(id)) return Forbid();
        if (year is < 2020 or > 2100 || month is < 1 or > 12) return Invalid("Ay veya yıl geçersiz.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        var plan = await db.Plans.Include(x => x.Days).SingleOrDefaultAsync(x =>
            x.ParticipationId == id && x.Year == year && x.Month == month);
        if (plan is null) return NotFound();
        if (!await db.Participations.AnyAsync(x => x.Id == id && x.Active)) return Invalid("Program katılımınız pasif.");
        if (Convert.ToBase64String(plan.Version) != version || plan.Status != "Submitted")
            return Changed("Planın güncel durumunu yeniden yükleyin.");
        var today = FinalRules.LocalDate(Now);
        if (!await EditingAllowed(id, year, month, today) ||
            plan.Days.Any(x => x.Decision == "Accepted" && x.Date <= today) ||
            await HasRecordedActivity(plan.Days.Select(x => x.Id)))
            return Changed("Düzenleme süresi kapalı veya çalışma planına bağlı kayıt var.");

        await Snapshot(plan, "BeforeWithdraw");
        plan.Status = "Draft";
        plan.UpdatedAt = Now;
        foreach (var day in plan.Days)
        {
            day.Decision = "Pending";
            day.Reason = null;
            day.DecidedById = null;
            day.DecidedAt = null;
        }
        Log("PlanWithdraw", plan.Id);
        await db.SaveChangesAsync();
        var view = await PlanView(id, year, month, plan);
        await transaction.CommitAsync();
        return Ok(view);
    }

    [HttpPut("plan-days/{dayId:int}/decision"), Authorize(Roles = Roles.Advisor + "," + Roles.Management)]
    public async Task<IActionResult> Decision(int dayId, DecisionDto input)
    {
        var id = await db.PlanDays.Where(x => x.Id == dayId).Select(x => (int?)x.MonthlyPlan.ParticipationId)
            .SingleOrDefaultAsync();
        if (id is null) return NotFound();

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id.Value);
        if (!await Can(id.Value)) return Forbid();
        var day = await db.PlanDays.Include(x => x.MonthlyPlan).ThenInclude(x => x.Days).SingleOrDefaultAsync(x => x.Id == dayId);
        if (day is null) return NotFound();
        var plan = day.MonthlyPlan;
        if (plan.Status != "Submitted" || input.Decision is not ("Accepted" or "Rejected"))
            return Invalid("Gönderilmiş plan ve geçerli karar gerekli.");
        if (Convert.ToBase64String(plan.Version) != input.Version) return Changed("Plan değişti. Yeniden yükleyin.");
        if ((input.Decision == "Rejected" || day.Conflict) && string.IsNullOrWhiteSpace(input.Reason))
            return Invalid("Ret veya ders çakışmasına rağmen kabul için gerekçe gerekli.");
        if (await HasRecordedActivity([dayId]))
            return Changed("Yoklama veya izin kaydı bulunan çalışma günü değiştirilemez.");
        if (input.Decision == "Accepted")
        {
            if (!await db.Participations.AnyAsync(p => p.Id == id && p.Active && p.Start <= day.Date && p.End >= day.Date))
                return Invalid("Çalışma tarihi aktif katılım aralığında olmalıdır.");
            var holiday = await db.Holidays.FindAsync(day.Date);
            if (holiday is not null && (!holiday.AfternoonOnly || day.Kind != "Morning"))
                return Invalid("Resmî tatil olan çalışma günü onaylanamaz.");
        }

        await Snapshot(plan, "BeforeDecision");
        day.Decision = input.Decision;
        day.Reason = input.Reason?.Trim();
        day.DecidedAt = Now;
        day.DecidedById = Uid;
        plan.UpdatedAt = Now;
        Log($"DayDecision:{input.Decision}", dayId);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }
}
