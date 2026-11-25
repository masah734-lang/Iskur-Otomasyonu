using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public partial class ApiController
{
    IQueryable<WorkUnit> QrUnits(User user)
    {
        var units = db.WorkUnits.Where(unit => unit.FacultyId == user.FacultyId);
        if (Management) return units;
        if (HeadRole) return units.Where(unit => unit.DepartmentId != null && unit.DepartmentId == user.DepartmentId);
        if (AdvisorRole)
            return units.Where(unit => db.Assignments.Any(a => a.WorkUnitId == unit.Id &&
                a.AdvisorId == user.Id && a.End == null && a.Participation.Active));
        return units.Where(unit => false);
    }

    [HttpGet("attendance/qr")]
    [Authorize(Roles = Roles.Management + "," + Roles.Head + "," + Roles.Advisor)]
    public async Task<IActionResult> IssueQr(int unitId, [FromServices] IDataProtectionProvider protection)
    {
        var me = await users.FindByIdAsync(Uid);
        if (me is null || !await QrUnits(me).AnyAsync(unit => unit.Id == unitId)) return Forbid();

        var now = Now;
        var today = FinalRules.LocalDate(now);
        var slot = ActiveQrSlot(now, today);
        if (slot is null) return Changed("Karekod, 08.00–08.20 ve 13.30–13.50 aralıklarında açılır.");

        var epoch = now.ToUnixTimeSeconds();
        var expires = epoch - epoch % FinalRules.QrPeriodSeconds + FinalRules.QrPeriodSeconds;
        var payload = new AttendanceToken(unitId, today, slot.Value, expires, AdvisorRole ? Uid : null);
        var token = protection.CreateProtector("Iskur.Attendance.v1").Protect(JsonSerializer.Serialize(payload));
        Response.Headers.CacheControl = "no-store";
        return Ok(new { token, expiresAt = DateTimeOffset.FromUnixTimeSeconds(expires), slot = slot.ToString() });
    }

    [HttpPost("students/{id:int}/attendance/check-in"), Authorize(Roles = Roles.Student)]
    public async Task<IActionResult> CheckIn(int id, QrInput input, [FromServices] IDataProtectionProvider protection)
    {
        if (!await Can(id)) return Forbid();
        AttendanceToken? code;
        try
        {
            var document = protection.CreateProtector("Iskur.Attendance.v1").Unprotect(input.Token);
            code = JsonSerializer.Deserialize<AttendanceToken>(document);
        }
        catch (Exception error) when (error is CryptographicException or JsonException or FormatException)
        {
            return Invalid("Karekod geçersiz.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        if (!await Can(id)) return Forbid();
        var now = Now;
        var today = FinalRules.LocalDate(now);
        if (code is null || code.Date != today || code.Expires <= now.ToUnixTimeSeconds() ||
            ActiveQrSlot(now, today) != code.Slot)
            return Invalid("Karekodun geçerlilik süresi dolmuş.");

        var assignment = await CurrentAssignment(id);
        if (assignment is null || assignment.WorkUnitId != code.UnitId ||
            (code.AdvisorId is not null && assignment.AdvisorId != code.AdvisorId))
            return Forbid();

        if (!await db.Participations.AnyAsync(x => x.Id == id && x.Active && x.Start <= today && x.End >= today))
            return Invalid("Aktif program katılımı bulunamadı.");

        var day = await AcceptedDays(id).SingleOrDefaultAsync(x => x.Date == today);
        var slots = day is null ? [] : FinalRules.Slots(day.Kind);
        if (day is null || !slots.Contains(code.Slot))
            return Invalid("Bu oturum için onaylanmış çalışma planınız yok.");

        var records = (await db.Set<SlotAttendance>().Where(x => x.PlanDayId == day.Id).ToListAsync())
            .ToDictionary(x => (x.PlanDayId, x.Slot));
        if (records.ContainsKey((day.Id, code.Slot)))
            return Changed("Bu oturum için daha önce yoklama veya izin kaydı oluşturulmuş.");

        RecordAttendance(day.Id, code.Slot, CheckInStatus.Present, "DirectQr", records);
        if (code.Slot == WorkSlot.Morning && slots.Contains(WorkSlot.Afternoon) &&
            !records.ContainsKey((day.Id, WorkSlot.Afternoon)))
            RecordAttendance(day.Id, WorkSlot.Afternoon, CheckInStatus.Present, "DailyQr", records);

        Log("AttendanceCheckIn", day.Id);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { date = today, slot = code.Slot.ToString() });
    }

    [HttpGet("students/{id:int}/attendance")]
    public async Task<IActionResult> Attendance(int id, int year, int? month)
    {
        if (!await Can(id)) return Forbid();
        if (year is < 2020 or > 2100 || month is < 1 or > 12) return Invalid("Ay veya yıl geçersiz.");
        var now = Now;
        var days = await AcceptedDays(id).Where(x => x.Date.Year == year && (!month.HasValue || x.Date.Month == month))
            .OrderBy(x => x.Date).ToListAsync();
        var records = await AttendanceFor(days.Select(x => x.Id));
        var rows = days.SelectMany(day => FinalRules.Slots(day.Kind).Select(slot =>
        {
            var record = records.GetValueOrDefault((day.Id, slot));
            var status = FinalRules.StatusAt(record, now, day.Date, slot);
            return new
            {
                planDayId = day.Id, day.Date, slot = slot.ToString(), status = status?.ToString() ?? "Pending",
                minutes = FinalRules.Minutes(slot), record?.Source, record?.RecordedAt
            };
        })).ToList();
        return Ok(new { plannedMinutes = rows.Sum(x => x.minutes),
            actualMinutes = rows.Where(x => x.status == nameof(CheckInStatus.Present)).Sum(x => x.minutes), rows });
    }

    [HttpPut("students/{id:int}/attendance/{dayId:int}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> CorrectAttendance(int id, int dayId, AttendanceCorrection input)
    {
        if (!Enum.TryParse<WorkSlot>(input.Slot, out var slot) || !Enum.IsDefined(slot) ||
            input.Status is not (nameof(CheckInStatus.Present) or nameof(CheckInStatus.Absent)) ||
            string.IsNullOrWhiteSpace(input.Reason))
            return Invalid("Oturum, durum ve düzeltme gerekçesi gereklidir.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        if (!await Can(id)) return Forbid();
        var day = await AcceptedDays(id).SingleOrDefaultAsync(x => x.Id == dayId);
        if (day is null || !FinalRules.Slots(day.Kind).Contains(slot)) return NotFound();
        var records = await AttendanceFor([dayId], tracking: true);
        var previous = records.GetValueOrDefault((dayId, slot));
        if (previous is null && Now < FinalRules.StartOf(day.Date, slot).AddMinutes(20))
            return Invalid("Karekod süresi dolmadan kayıtsız oturum düzeltilemez.");
        if (previous?.Status == CheckInStatus.OnLeave)
            return Changed("Onaylı izin yoklama düzeltmesiyle değiştirilemez.");

        var previousStatus = FinalRules.StatusAt(previous, Now, day.Date, slot)?.ToString() ?? "Pending";
        RecordAttendance(dayId, slot, Enum.Parse<CheckInStatus>(input.Status), "Manual", records);
        Log("AttendanceCorrection", $"{dayId}/{slot}/{previousStatus}/{input.Status}: {input.Reason.Trim()}");
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }

    async Task<Dictionary<(int, WorkSlot), SlotAttendance>> AttendanceFor(IEnumerable<int> ids, bool tracking = false)
    {
        var source = db.Set<SlotAttendance>().AsQueryable();
        if (!tracking) source = source.AsNoTracking();
        var keys = ids.ToArray();
        return await source.Where(x => keys.Contains(x.PlanDayId)).ToDictionaryAsync(x => (x.PlanDayId, x.Slot));
    }

    void RecordAttendance(int dayId, WorkSlot slot, CheckInStatus status, string source,
        Dictionary<(int, WorkSlot), SlotAttendance> records)
    {
        if (!records.TryGetValue((dayId, slot), out var attendance))
        {
            attendance = new SlotAttendance { PlanDayId = dayId, Slot = slot };
            db.Add(attendance);
            records.Add((dayId, slot), attendance);
        }

        attendance.Status = status;
        attendance.Source = source;
        attendance.ActorId = Uid;
        attendance.RecordedAt = Now;
    }

    public static WorkSlot? ActiveQrSlot(DateTimeOffset now, DateOnly date)
    {
        foreach (var slot in Enum.GetValues<WorkSlot>())
        {
            var starts = FinalRules.StartOf(date, slot);
            if (now >= starts && now < starts.AddMinutes(20)) return slot;
        }

        return null;
    }

    sealed record AttendanceToken(int UnitId, DateOnly Date, WorkSlot Slot, long Expires, string? AdvisorId);
}

public sealed record QrInput([Required, StringLength(2048)] string Token);
public sealed record AttendanceCorrection(string Slot, string Status, [Required, StringLength(500)] string Reason);
