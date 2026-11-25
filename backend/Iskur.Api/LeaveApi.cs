using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public partial class ApiController
{
    const int MaxLeaveFile = 10 * 1024 * 1024;

    async Task<decimal> LeaveDaysWithStatus(int participation, string status) =>
        await db.Set<LeaveSlot>().Where(x => x.Request.ParticipationId == participation &&
            x.Request.Status == status).SumAsync(x => x.ChargedDays);

    [HttpGet("students/{id:int}/leaves")]
    public async Task<IActionResult> Leaves(int id)
    {
        if (!await Can(id)) return Forbid();
        var policy = await Policy(await ParticipationFaculty(id));
        var used = await LeaveDaysWithStatus(id, "Approved");
        var pending = await LeaveDaysWithStatus(id, "Pending");
        var rows = await db.Set<LeaveRequest>().Where(x => x.ParticipationId == id)
            .OrderByDescending(x => x.Id).Select(x => new
            {
                x.Id, x.Start, x.End, x.Type, reason = Management || StudentRole ? x.Reason : null, x.Status, x.Minutes, x.ReviewedAt,
                reviewer = x.Reviewer == null ? null : x.Reviewer.FullName,
                days = db.Set<LeaveSlot>().Where(s => s.LeaveRequestId == x.Id).Sum(s => s.ChargedDays),
                hasFile = (Management || StudentRole) && db.Set<LeaveAttachment>().Any(f => f.LeaveRequestId == x.Id),
                history = db.Set<LeaveDecision>().Where(a => a.LeaveRequestId == x.Id)
                    .OrderBy(a => a.Id).Select(a => new { a.Status, a.RecordedAt, note = Management || StudentRole ? a.Note : null }).ToList()
            }).ToListAsync();

        Log("LeaveRead", id);
        await db.SaveChangesAsync();
        return Ok(new
        {
            entitlement = policy.LeaveDays, used, pending, remaining = policy.LeaveDays - used,
            warning = policy.LeaveDays - used - pending <= policy.LeaveWarningDays, rows
        });
    }

    [HttpPost("students/{id:int}/leaves"), Authorize(Roles = Roles.Student)]
    [RequestSizeLimit(MaxLeaveFile + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxLeaveFile + 65536)]
    public async Task<IActionResult> ApplyLeave(int id, [FromForm] LeaveInput request)
    {
        if (!await Can(id)) return Forbid();
        var today = FinalRules.LocalDate(Now);
        if (request.Start > request.End || request.End.DayNumber - request.Start.DayNumber > 366 ||
            request.Type is not ("Personal" or "Medical") || string.IsNullOrWhiteSpace(request.Reason))
            return Invalid("İzin türünü, gerekçeyi ve tarihleri kontrol edin.");

        if (request.Start < today && request.Type != "Medical")
            return Invalid("Mazeret izni geçmiş çalışma günlerine açılamaz.");

        byte[]? bytes = null;
        string? contentType = null;
        if (request.File is not null)
        {
            if (request.File.Length is < 1 or > MaxLeaveFile)
                return Invalid("İzin belgesi en fazla 10 MB olabilir.");
            await using var stream = new MemoryStream();
            await request.File.CopyToAsync(stream);
            bytes = stream.ToArray();
            contentType = LeaveFileType(bytes);
            if (contentType is null) return Invalid("PDF, JPG veya PNG belgesi yükleyin.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        if (!await Can(id)) return Forbid();
        if (!await db.Participations.AnyAsync(x => x.Id == id && x.Active &&
            x.Start <= request.Start && x.End >= request.End))
            return Invalid("İzin tarihleri aktif program katılımının içinde olmalıdır.");

        var days = await AcceptedDays(id).Where(x => x.Date >= request.Start && x.Date <= request.End).ToListAsync();
        if (days.Count == 0) return Invalid("Bu aralıkta onaylanmış çalışma günü bulunamadı.");

        var dayIds = days.Select(x => x.Id).ToList();
        var alreadyRequested = await db.Set<LeaveSlot>().AnyAsync(x => dayIds.Contains(x.PlanDayId) &&
            (x.Request.Status == "Pending" || x.Request.Status == "Approved"));
        if (alreadyRequested) return Changed("Bu aralıkta bekleyen veya onaylanmış izin bulunmaktadır.");

        if (await db.Set<SlotAttendance>().AnyAsync(x => dayIds.Contains(x.PlanDayId) && x.Status == CheckInStatus.Present))
            return Invalid("Yoklaması alınmış güne izin talebi oluşturulamaz.");

        var policy = await Policy(await ParticipationFaculty(id));
        var leave = new LeaveRequest
        {
            ParticipationId = id, Start = request.Start, End = request.End, Type = request.Type,
            Reason = request.Reason.Trim(), Minutes = days.Sum(x => x.Minutes)
        };
        db.Add(leave);

        var chargedDays = request.Type == "Medical" && policy.MedicalLeaveExempt ? 0m : 0.5m;
        foreach (var day in days)
            foreach (var slot in FinalRules.Slots(day.Kind))
                db.Add(new LeaveSlot { Request = leave, PlanDayId = day.Id, Slot = slot, ChargedDays = chargedDays });

        if (bytes is not null)
            db.Add(new LeaveAttachment { Request = leave, ContentType = contentType!, Bytes = bytes });

        db.Add(new LeaveDecision { Request = leave, Status = "Pending", ActorId = Uid, RecordedAt = Now });
        await db.SaveChangesAsync();
        Log("LeaveRequested", leave.Id);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { leave.Id });
    }

    [HttpGet("students/{id:int}/leaves/{requestId:int}/attachment")]
    [Authorize(Roles = Roles.Student + "," + Roles.Management)]
    public async Task<IActionResult> DownloadLeaveFile(int id, int requestId)
    {
        if (!await Can(id)) return Forbid();
        var file = await db.Set<LeaveAttachment>()
            .SingleOrDefaultAsync(x => x.LeaveRequestId == requestId && x.Request.ParticipationId == id);
        if (file is null) return NotFound();
        Log("LeaveAttachmentRead", requestId);
        await db.SaveChangesAsync();
        var extension = file.ContentType switch { "application/pdf" => "pdf", "image/png" => "png", _ => "jpg" };
        Response.Headers.CacheControl = "no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(file.Bytes, file.ContentType, $"izin-belgesi-{requestId}.{extension}");
    }

    [HttpPost("students/{id:int}/leaves/{requestId:int}/decision")]
    [Authorize(Roles = Roles.Student + "," + Roles.Management)]
    public async Task<IActionResult> ReviewLeave(int id, int requestId, LeaveReview request)
    {
        if (!await Can(id)) return Forbid();
        if (request.Status == "Cancelled" ? !StudentRole : !Management) return Forbid();
        if (request.Status is not ("Approved" or "Rejected" or "Cancelled") ||
            (request.Status == "Rejected" && string.IsNullOrWhiteSpace(request.Note)))
            return Invalid("Geçerli bir karar girin. Ret işlemi için gerekçe zorunludur.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        if (!await Can(id)) return Forbid();
        var leave = await db.Set<LeaveRequest>().SingleOrDefaultAsync(x => x.Id == requestId && x.ParticipationId == id);
        if (leave is null) return NotFound();
        if (leave.Status != "Pending") return Changed("Bu talep daha önce sonuçlandırılmış.");

        if (request.Status == "Approved")
        {
            var slots = await db.Set<LeaveSlot>().Include(x => x.PlanDay).ThenInclude(x => x.MonthlyPlan)
                .Where(x => x.LeaveRequestId == requestId).ToListAsync();
            if (slots.Count == 0 || slots.Any(x => x.PlanDay.Decision != "Accepted" ||
                x.PlanDay.MonthlyPlan.Status != "Submitted"))
                return Changed("İzin talebine bağlı çalışma planı değişmiş.");

            var policy = await Policy(await ParticipationFaculty(id));
            var usedDays = await LeaveDaysWithStatus(id, "Approved");
            var requestedDays = slots.Sum(x => x.ChargedDays);
            if (requestedDays > 0 && usedDays + requestedDays > policy.LeaveDays)
                return Changed("Talep kalan izin hakkını aşıyor.");

            var ids = slots.Select(x => x.PlanDayId).Distinct().ToList();
            var records = (await db.Set<SlotAttendance>().Where(x => ids.Contains(x.PlanDayId)).ToListAsync())
                .ToDictionary(x => (x.PlanDayId, x.Slot));
            if (slots.Any(x => records.TryGetValue((x.PlanDayId, x.Slot), out var attendance) &&
                attendance.Status != CheckInStatus.Absent))
                return Changed("Bu tarihte mevcut yoklama veya onaylı izin var.");

            foreach (var slot in slots)
                RecordAttendance(slot.PlanDayId, slot.Slot, CheckInStatus.OnLeave, "Leave", records);
        }

        leave.Status = request.Status;
        leave.ReviewerId = Management ? Uid : null;
        leave.ReviewedAt = Management ? Now : null;
        db.Add(new LeaveDecision
        {
            LeaveRequestId = requestId, Status = request.Status, Note = request.Note?.Trim(), ActorId = Uid, RecordedAt = Now
        });
        Log($"Leave{request.Status}", requestId);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }

    static string? LeaveFileType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith("%PDF-"u8)) return "application/pdf";
        if (bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        return bytes.StartsWith(new byte[] { 255, 216, 255 }) ? "image/jpeg" : null;
    }
}

public sealed record LeaveInput(DateOnly Start, DateOnly End, string Type, [Required, StringLength(1000)] string Reason, IFormFile? File);
public sealed record LeaveReview(string Status, [StringLength(500)] string? Note);
