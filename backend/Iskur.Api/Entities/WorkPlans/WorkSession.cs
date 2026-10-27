using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Scheduling;
using Iskur.Api.Entities.Students;

namespace Iskur.Api.Entities.WorkPlans;

public class WorkSession : BaseEntity
{
    public Guid WorkPlanDayId { get; set; }

    public SessionType SessionType { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public int PlannedMinutes { get; set; }

    public Guid UnitAssignmentId { get; set; }

    public Guid AdvisorAssignmentId { get; set; }

    public Guid WorkRuleId { get; set; }

    public Guid? CourseScheduleId { get; set; }

    public bool HasCourseConflict { get; set; }

    public string? ConflictDetails { get; set; }

    public WorkPlanDay WorkPlanDay { get; set; } = null!;

    public UnitAssignment UnitAssignment { get; set; } = null!;

    public AdvisorAssignment AdvisorAssignment { get; set; } = null!;

    public WorkRule WorkRule { get; set; } = null!;

    public CourseSchedule? CourseSchedule { get; set; }
}