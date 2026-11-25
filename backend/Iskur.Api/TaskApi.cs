using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public partial class ApiController
{
    [HttpGet("students/{id:int}/tasks")]
    public async Task<IActionResult> Tasks(int id, bool? completed)
    {
        if (!await Can(id)) return Forbid();
        return Ok(await db.Set<StudentTask>().Where(x => x.Assignment.ParticipationId == id &&
            (!completed.HasValue || x.Completed == completed))
            .AsSplitQuery().OrderBy(x => x.DueDate).ThenByDescending(x => x.Id).Select(x => new
            {
                x.Id, x.Description, x.DueDate, x.Completed,
                advisor = x.Assignment.Advisor == null ? null : x.Assignment.Advisor.FullName,
                notes = db.Set<Evaluation>().Where(e => e.StudentTaskId == x.Id).OrderBy(e => e.At)
                    .Select(e => new { e.Note, e.At, author = e.Author.FullName }).ToList(),
                history = db.Set<TaskHistory>().Where(h => h.StudentTaskId == x.Id).OrderBy(h => h.Id)
                    .Select(h => new { h.Action, h.Completed, h.RecordedAt }).ToList()
            }).ToListAsync());
    }

    [HttpPost("students/{id:int}/tasks"), Authorize(Roles = Roles.Advisor)]
    public async Task<IActionResult> AddTask(int id, TaskInput input)
    {
        if (!await Can(id)) return Forbid();
        if (string.IsNullOrWhiteSpace(input.Description) || input.DueDate < FinalRules.LocalDate(Now))
            return Invalid("Görevi açıklayın ve geçerli bir hedef tarih seçin.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        var assignment = await db.Assignments.Include(x => x.Participation)
            .SingleOrDefaultAsync(x => x.ParticipationId == id && x.End == null && x.AdvisorId == Uid);
        if (assignment is null) return Forbid();
        if (!assignment.Participation.Active || input.DueDate < assignment.Participation.Start ||
            input.DueDate > assignment.Participation.End)
            return Invalid("Hedef tarih aktif katılım dönemi içinde olmalıdır.");

        var task = new StudentTask
        {
            AssignmentId = assignment.Id, Description = input.Description.Trim(), DueDate = input.DueDate
        };
        db.Add(task);
        db.Add(new TaskHistory { Task = task, Action = "Created", ActorId = Uid, RecordedAt = Now });
        await db.SaveChangesAsync();
        Log("TaskCreated", task.Id);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { task.Id });
    }

    [HttpPost("students/{id:int}/tasks/{taskId:int}/evaluation"), Authorize(Roles = Roles.Advisor)]
    public async Task<IActionResult> EvaluateTask(int id, int taskId, EvaluationInput input)
    {
        if (!await Can(id)) return Forbid();
        if (!input.Completed.HasValue && string.IsNullOrWhiteSpace(input.Note))
            return Invalid("Bir durum veya değerlendirme notu girin.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        if (!await Can(id)) return Forbid();
        var task = await db.Set<StudentTask>().SingleOrDefaultAsync(x => x.Id == taskId &&
            x.Assignment.ParticipationId == id);
        if (task is null) return NotFound();

        var changed = input.Completed.HasValue && task.Completed != input.Completed.Value;
        if (changed) task.Completed = input.Completed!.Value;
        if (!string.IsNullOrWhiteSpace(input.Note))
            db.Add(new Evaluation { StudentTaskId = taskId, AuthorId = Uid, Note = input.Note.Trim(), At = Now });

        if (changed || !string.IsNullOrWhiteSpace(input.Note))
        {
            db.Add(new TaskHistory { StudentTaskId = taskId, Action = changed ? "Status" : "Note",
                ActorId = Uid, Completed = task.Completed, RecordedAt = Now });
            Log("TaskEvaluated", taskId);
            await db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        return Ok();
    }

    [HttpGet("students/{id:int}/absence-reports")]
    [Authorize(Roles = Roles.Advisor + "," + Roles.Head + "," + Roles.Management)]
    public async Task<IActionResult> AbsenceReports(int id)
    {
        if (!await Can(id)) return Forbid();
        return Ok(await db.Set<AbsenceReport>().Where(x => x.ParticipationId == id &&
            (!Management || x.Status != "Reported")).OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.PlanDay.Date, x.Status, x.Reason, x.Resolution, x.CreatedAt,
                history = db.Set<AbsenceReportAction>().Where(a => a.AbsenceReportId == x.Id).OrderBy(a => a.Id)
                    .Select(a => new { a.Status, a.Reason, a.RecordedAt }).ToList() }).ToListAsync());
    }

    [HttpPost("students/{id:int}/absence-reports"), Authorize(Roles = Roles.Advisor)]
    public async Task<IActionResult> ReportAbsence(int id, AbsenceInput input)
    {
        if (!await Can(id)) return Forbid();
        if (string.IsNullOrWhiteSpace(input.Reason)) return Invalid("Bildirim gerekçesi gereklidir.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        if (!await Can(id)) return Forbid();

        var day = await AcceptedDays(id).SingleOrDefaultAsync(x => x.Id == input.PlanDayId);
        if (day is null) return NotFound();
        var records = await AttendanceFor([day.Id]);
        if (!FinalRules.Slots(day.Kind).Any(slot => FinalRules.StatusAt(records.GetValueOrDefault((day.Id, slot)),
                Now, day.Date, slot) == CheckInStatus.Absent))
            return Invalid("Bu gün için devamsız oturum bulunamadı.");
        if (await db.Set<AbsenceReport>().AnyAsync(x => x.PlanDayId == day.Id))
            return Changed("Bu gün için zaten devamsızlık bildirimi var.");

        var assignment = await CurrentAssignment(id);
        var status = assignment?.WorkUnit.DepartmentId is null ? "Forwarded" : "Reported";
        var report = new AbsenceReport { PlanDayId = day.Id, ParticipationId = id, Status = status,
            Reason = input.Reason.Trim(), CreatedById = Uid, CreatedAt = Now };
        db.Add(report);
        db.Add(new AbsenceReportAction { Report = report, Status = status, Reason = report.Reason,
            ActorId = Uid, RecordedAt = Now });
        Log("AbsenceReported", day.Id);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new { report.Id });
    }

    [HttpPost("students/{id:int}/absence-reports/{reportId:int}/decision")]
    [Authorize(Roles = Roles.Head + "," + Roles.Management)]
    public async Task<IActionResult> ReviewAbsence(int id, int reportId, AbsenceReview input)
    {
        if (!await Can(id)) return Forbid();
        if (string.IsNullOrWhiteSpace(input.Reason)) return Invalid("Karar gerekçesi gereklidir.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.LockParticipation(id);
        if (!await Can(id)) return Forbid();

        var report = await db.Set<AbsenceReport>().Include(x => x.Participation)
            .SingleOrDefaultAsync(x => x.Id == reportId && x.ParticipationId == id);
        if (report is null) return NotFound();
        if (HeadRole && (input.Status is not ("Forwarded" or "Closed") || report.Status != "Reported") ||
            Management && (input.Status is not ("Closed" or "Terminated") || report.Status != "Forwarded"))
            return Changed("Bu durum için geçerli bir karar seçin.");

        if (input.Status == "Terminated")
        {
            var p = report.Participation;
            var today = FinalRules.LocalDate(Now);
            if (!p.Active || today < p.Start || today > p.End)
                return Invalid("Yalnız aktif program katılımı sonlandırılabilir.");
            p.Active = false;
            p.End = today;
        }

        report.Status = input.Status;
        report.Resolution = input.Reason.Trim();
        db.Add(new AbsenceReportAction { AbsenceReportId = reportId, Status = input.Status,
            Reason = report.Resolution, ActorId = Uid, RecordedAt = Now });
        Log($"Absence{input.Status}", reportId);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }
}

public sealed record TaskInput([Required, StringLength(1000)] string Description, DateOnly DueDate);
public sealed record EvaluationInput(bool? Completed, [StringLength(1000)] string? Note);
public sealed record AbsenceInput(int PlanDayId, [Required, StringLength(500)] string Reason);
public sealed record AbsenceReview(string Status, [Required, StringLength(500)] string Reason);
