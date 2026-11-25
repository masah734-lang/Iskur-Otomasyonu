using Microsoft.EntityFrameworkCore;

namespace Iskur.Api;

public enum WorkSlot { Morning, Afternoon }
public enum CheckInStatus { Present, Absent, OnLeave }

public sealed class SlotAttendance
{
    public int PlanDayId { get; set; }
    public PlanDay PlanDay { get; set; } = null!;
    public WorkSlot Slot { get; set; }
    public CheckInStatus Status { get; set; }
    public string Source { get; set; } = "";
    public string ActorId { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
}

public sealed class LeaveSlot
{
    public int LeaveRequestId { get; set; }
    public LeaveRequest Request { get; set; } = null!;
    public int PlanDayId { get; set; }
    public PlanDay PlanDay { get; set; } = null!;
    public WorkSlot Slot { get; set; }
    public decimal ChargedDays { get; set; }
}

public sealed class LeaveDecision
{
    public long Id { get; set; }
    public int LeaveRequestId { get; set; }
    public LeaveRequest Request { get; set; } = null!;
    public string Status { get; set; } = "";
    public string? Note { get; set; }
    public string ActorId { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
}

public sealed class LeaveAttachment
{
    public int LeaveRequestId { get; set; }
    public LeaveRequest Request { get; set; } = null!;
    public string ContentType { get; set; } = "";
    public byte[] Bytes { get; set; } = [];
}

public sealed class FacultyPolicy
{
    public int FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;
    public int LeaveDays { get; set; } = 10;
    public int LeaveWarningDays { get; set; } = 2;
    public int AbsenceWarningDays { get; set; } = 3;
    public bool MedicalLeaveExempt { get; set; }
}

public sealed class PlanWindow
{
    public int FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public DateOnly Opens { get; set; }
    public DateOnly Closes { get; set; }
}

public sealed class PlanExamDate
{
    public int MonthlyPlanId { get; set; }
    public MonthlyPlan MonthlyPlan { get; set; } = null!;
    public DateOnly Date { get; set; }
}

public sealed class PlanRevision
{
    public long Id { get; set; }
    public int MonthlyPlanId { get; set; }
    public MonthlyPlan MonthlyPlan { get; set; } = null!;
    public string Action { get; set; } = "";
    public string ActorId { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
    public string Snapshot { get; set; } = "";
}

public sealed class TaskHistory
{
    public long Id { get; set; }
    public int StudentTaskId { get; set; }
    public StudentTask Task { get; set; } = null!;
    public string Action { get; set; } = "";
    public string ActorId { get; set; } = "";
    public bool Completed { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}

public sealed class AbsenceReport
{
    public int Id { get; set; }
    public int ParticipationId { get; set; }
    public Participation Participation { get; set; } = null!;
    public int PlanDayId { get; set; }
    public PlanDay PlanDay { get; set; } = null!;
    public string Status { get; set; } = "";
    public string Reason { get; set; } = "";
    public string CreatedById { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public string? Resolution { get; set; }
}

public sealed class AbsenceReportAction
{
    public long Id { get; set; }
    public int AbsenceReportId { get; set; }
    public AbsenceReport Report { get; set; } = null!;
    public string Status { get; set; } = "";
    public string Reason { get; set; } = "";
    public string ActorId { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
}

public sealed class CourseRevision
{
    public long Id { get; set; }
    public int ParticipationId { get; set; }
    public Participation Participation { get; set; } = null!;
    public DateTimeOffset RecordedAt { get; set; }
    public string Snapshot { get; set; } = "";
}

public partial class AppDb
{
    internal static void ConfigureFinal(ModelBuilder model)
    {
        model.Entity<SlotAttendance>().HasKey(x => new { x.PlanDayId, x.Slot });
        model.Entity<LeaveSlot>().HasKey(x => new { x.LeaveRequestId, x.PlanDayId, x.Slot });
        model.Entity<LeaveSlot>().Property(x => x.ChargedDays).HasPrecision(5, 2);
        model.Entity<LeaveDecision>();
        model.Entity<LeaveAttachment>().HasKey(x => x.LeaveRequestId);
        model.Entity<LeaveAttachment>().HasOne(x => x.Request).WithOne().HasForeignKey<LeaveAttachment>(x => x.LeaveRequestId);
        model.Entity<LeaveAttachment>().Property(x => x.LeaveRequestId).ValueGeneratedNever();
        model.Entity<FacultyPolicy>().HasKey(x => x.FacultyId);
        model.Entity<FacultyPolicy>().HasOne(x => x.Faculty).WithOne().HasForeignKey<FacultyPolicy>(x => x.FacultyId);
        model.Entity<FacultyPolicy>().Property(x => x.FacultyId).ValueGeneratedNever();
        model.Entity<PlanWindow>().HasKey(x => new { x.FacultyId, x.Year, x.Month });
        model.Entity<PlanExamDate>().HasKey(x => new { x.MonthlyPlanId, x.Date });
        model.Entity<PlanRevision>();
        model.Entity<TaskHistory>();
        model.Entity<AbsenceReport>().HasIndex(x => x.PlanDayId).IsUnique();
        model.Entity<AbsenceReportAction>();
        model.Entity<CourseRevision>();
    }
}
