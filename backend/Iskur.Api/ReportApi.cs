using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public partial class ApiController
{
    [HttpGet("reports/work")]
    public async Task<IActionResult> WorkReport([FromQuery] WorkReportFilter filter, bool csv = false)
    {
        if (filter.Year is < 2020 or > 2100 || filter.Month is < 1 or > 12 ||
            filter.AttendanceStatus is not (null or "Present" or "Absent" or "OnLeave" or "Pending"))
            return Invalid("Rapor filtrelerini kontrol edin.");
        if (csv && !Management && !HeadRole) return Forbid();

        var first = new DateOnly(filter.Year, filter.Month ?? 1, 1);
        var last = filter.Month.HasValue ? first.AddMonths(1) : first.AddYears(1);
        var participation = Scope();
        if (filter.ParticipationId.HasValue) participation = participation.Where(x => x.Id == filter.ParticipationId);
        if (filter.DepartmentId.HasValue) participation = participation.Where(x => x.Student.DepartmentId == filter.DepartmentId);
        if (filter.Active.HasValue) participation = participation.Where(x => x.Active == filter.Active);

        var people = await participation.AsNoTracking().Select(x => new
        {
            x.Id, x.Student.User.FullName, x.Student.StudentNumber, department = x.Student.Department.Name,
            nationalId = Management ? x.Student.NationalId : null, iban = Management ? x.Student.Iban : null,
            x.End
        }).ToDictionaryAsync(x => x.Id);
        var query = db.PlanDays.AsNoTracking().Where(x => x.Date >= first && x.Date < last &&
            x.Date <= x.MonthlyPlan.Participation.End &&
            x.MonthlyPlan.Status == "Submitted" && x.Decision == "Accepted" &&
            participation.Any(p => p.Id == x.MonthlyPlan.ParticipationId));
        var days = await query.Select(x => new
            { x.Id, x.Date, x.Kind, x.MonthlyPlan.ParticipationId }).ToListAsync();
        var history = await db.Assignments.AsNoTracking().Where(x => participation.Any(p => p.Id == x.ParticipationId))
            .OrderBy(x => x.Start).Select(x => new
            {
                x.ParticipationId, x.WorkUnitId, x.AdvisorId, x.Start, x.End,
                unit = x.WorkUnit.Name, advisor = x.Advisor == null ? null : x.Advisor.FullName
            }).ToListAsync();
        var byParticipation = history.ToLookup(x => x.ParticipationId);
        var attendance = await AttendanceFor(days.Select(x => x.Id));
        var now = Now;

        var work = (from day in days
                    let assignments = byParticipation[day.ParticipationId]
                    from slot in FinalRules.Slots(day.Kind)
                    let onDate = FinalRules.StartOf(day.Date, slot)
                    let assignment = assignments.LastOrDefault(a => a.Start <= onDate && (a.End == null || onDate < a.End))
                        ?? assignments.FirstOrDefault()
                    where assignment is not null &&
                        (!filter.WorkUnitId.HasValue || assignment.WorkUnitId == filter.WorkUnitId) &&
                        (filter.AdvisorId is null || assignment.AdvisorId == filter.AdvisorId)
                    let status = FinalRules.StatusAt(attendance.GetValueOrDefault((day.Id, slot)), now, day.Date, slot)
                    where filter.AttendanceStatus is null || (status?.ToString() ?? "Pending") == filter.AttendanceStatus
                    select new { day.ParticipationId, day.Date, assignment.WorkUnitId, assignment.unit,
                        assignment.AdvisorId, assignment.advisor, status, minutes = FinalRules.Minutes(slot) }).ToList();

        var rows = work.GroupBy(x => new { x.ParticipationId, x.Date.Year, x.Date.Month, x.WorkUnitId,
                x.AdvisorId, x.unit, x.advisor })
            .Select(group =>
            {
                var person = people[group.Key.ParticipationId];
                var present = group.Where(x => x.status == CheckInStatus.Present).ToList();
                return new WorkReportRow(person.Id, person.FullName, person.StudentNumber, person.department,
                    person.nationalId, person.iban, group.Key.unit, group.Key.advisor,
                    group.Key.Year, group.Key.Month, group.Select(x => x.Date).Distinct().Order().ToArray(),
                    present.Select(x => x.Date).Distinct().Order().ToArray(), group.Sum(x => x.minutes),
                    present.Sum(x => x.minutes), group.Where(x => x.status == CheckInStatus.OnLeave)
                        .Select(x => x.Date).Distinct().Count(),
                    group.Where(x => x.status == CheckInStatus.Absent).Select(x => x.Date).Distinct().Count());
            }).OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.FullName).ThenBy(x => x.Unit).ToList();

        Log(csv ? "WorkReportExport" : "WorkReportRead", $"{filter.Year}/{filter.Month}");
        await db.SaveChangesAsync();
        if (!csv) return Ok(rows);
        Response.Headers.CacheControl = "no-store";
        return File(ReportCsv(rows, Management), "text/csv; charset=utf-8", $"Iskur-Calisma-{filter.Year}-{filter.Month ?? 0:D2}.csv");
    }

    [HttpGet("reports/tasks"), Authorize(Roles = Roles.Advisor + "," + Roles.Head + "," + Roles.Management)]
    public async Task<IActionResult> TaskReport(int year, int? month, int? participationId, int? departmentId,
        int? workUnitId, string? advisorId, bool? active, bool? completed)
    {
        if (year is < 2020 or > 2100 || month is < 1 or > 12) return Invalid("Ay veya yıl geçersiz.");
        var scope = Scope();
        if (departmentId.HasValue) scope = scope.Where(p => p.Student.DepartmentId == departmentId);
        if (active.HasValue) scope = scope.Where(p => p.Active == active);
        return Ok(await db.Set<StudentTask>().Where(task => scope.Any(p => p.Id == task.Assignment.ParticipationId) &&
            task.DueDate.Year == year && (!month.HasValue || task.DueDate.Month == month) &&
            (!participationId.HasValue || task.Assignment.ParticipationId == participationId) &&
            (!workUnitId.HasValue || task.Assignment.WorkUnitId == workUnitId) &&
            (advisorId == null || task.Assignment.AdvisorId == advisorId) &&
            (!completed.HasValue || task.Completed == completed))
            .OrderBy(task => task.DueDate).ThenBy(task => task.Id).Select(task => new
            {
                task.Id, task.Description, task.DueDate, task.Completed,
                fullName = task.Assignment.Participation.Student.User.FullName,
                advisor = task.Assignment.Advisor == null ? null : task.Assignment.Advisor.FullName,
                assessmentCount = db.Set<Evaluation>().Count(e => e.StudentTaskId == task.Id)
            }).ToListAsync());
    }

    [HttpGet("reports/warnings"), Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> Warnings()
    {
        var faculty = await MyFaculty();
        if (!faculty.HasValue) return Forbid();
        var policy = await Policy(faculty.Value);
        var today = FinalRules.LocalDate(Now);
        var people = await Scope().Where(x => x.Active && x.Start <= today && x.End >= today)
            .Select(x => new { x.Id, x.Student.User.FullName, x.Student.StudentNumber }).ToListAsync();
        var ids = people.Select(x => x.Id).ToArray();
        var allDays = await db.PlanDays.Where(d => ids.Contains(d.MonthlyPlan.ParticipationId) &&
            d.MonthlyPlan.Status == "Submitted" && d.Decision == "Accepted" && d.Date <= today)
            .Select(d => new { d.Id, d.MonthlyPlan.ParticipationId, d.Date, d.Kind }).ToListAsync();
        var attendance = await AttendanceFor(allDays.Select(x => x.Id));
        var leave = await db.Set<LeaveSlot>().Where(s => ids.Contains(s.Request.ParticipationId) &&
            (s.Request.Status == "Approved" || s.Request.Status == "Pending"))
            .GroupBy(s => new { s.Request.ParticipationId, s.Request.Status })
            .Select(g => new { g.Key.ParticipationId, g.Key.Status, days = g.Sum(s => s.ChargedDays) })
            .ToDictionaryAsync(g => (g.ParticipationId, g.Status), g => g.days);
        var daysByStudent = allDays.ToLookup(d => d.ParticipationId);
        var now = Now;
        var rows = people.Select(p =>
        {
            var used = leave.GetValueOrDefault((p.Id, "Approved"));
            var pending = leave.GetValueOrDefault((p.Id, "Pending"));
            var absent = daysByStudent[p.Id].Count(d => FinalRules.Slots(d.Kind).Any(s =>
                FinalRules.StatusAt(attendance.GetValueOrDefault((d.Id, s)), now, d.Date, s) == CheckInStatus.Absent));
            return new { p.Id, p.FullName, p.StudentNumber, used, pending, remaining = policy.LeaveDays - used,
                absent, leaveWarning = policy.LeaveDays - used - pending <= policy.LeaveWarningDays,
                absenceWarning = absent >= policy.AbsenceWarningDays };
        }).Where(p => p.leaveWarning || p.absenceWarning).OrderBy(p => p.FullName).ToList();
        return Ok(rows);
    }

    static byte[] ReportCsv(IEnumerable<WorkReportRow> rows, bool includeIdentity)
    {
        var csv = new StringBuilder();
        csv.AppendLine((includeIdentity ? "T.C. kimlik numarası;IBAN;" : "") +
            "Öğrenci;Öğrenci numarası;Bölüm;Çalışma birimi;Danışman;Yıl;Ay;Planlanan tarihler;Gerçekleşen tarihler;Planlanan dakika;Gerçekleşen dakika;Gerçekleşen saat;İzinli gün;Devamsız gün");
        foreach (var row in rows)
        {
            var cells = new List<string>();
            if (includeIdentity) cells.AddRange([row.NationalId ?? "", row.Iban ?? ""]);
            cells.AddRange([row.FullName, row.StudentNumber, row.Department, row.Unit, row.Advisor ?? "",
                row.Year.ToString(CultureInfo.InvariantCulture), row.Month.ToString(CultureInfo.InvariantCulture),
                Dates(row.PlannedDates), Dates(row.ActualDates), row.PlannedMinutes.ToString(CultureInfo.InvariantCulture),
                row.ActualMinutes.ToString(CultureInfo.InvariantCulture),
                (row.ActualMinutes / 60m).ToString("0.##", CultureInfo.GetCultureInfo("tr-TR")),
                row.LeaveDays.ToString(CultureInfo.InvariantCulture), row.AbsentDays.ToString(CultureInfo.InvariantCulture)]);
            csv.AppendLine(string.Join(';', cells.Select(CsvCell)));
        }
        return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
    }

    static string Dates(IEnumerable<DateOnly> dates) =>
        string.Join(", ", dates.Select(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

    static string CsvCell(string value)
    {
        var first = value.AsSpan().TrimStart();
        if (first.Length > 0 && first[0] is '=' or '+' or '-' or '@' || value.StartsWith('\t') || value.StartsWith('\r'))
            value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}

public sealed record WorkReportFilter(int Year, int? Month, int? ParticipationId, int? DepartmentId,
    int? WorkUnitId, string? AdvisorId, bool? Active, string? AttendanceStatus);

public sealed record WorkReportRow(int ParticipationId, string FullName, string StudentNumber, string Department,
    string? NationalId, string? Iban, string Unit, string? Advisor, int Year, int Month,
    DateOnly[] PlannedDates, DateOnly[] ActualDates, int PlannedMinutes, int ActualMinutes, int LeaveDays, int AbsentDays);
