using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Students;

namespace Iskur.Api.Entities.WorkPlans;

public class MonthlyWorkPlan : BaseEntity
{
    public Guid ParticipationId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public PlanStatus Status { get; set; } = PlanStatus.Draft;

    public DateTimeOffset? SubmittedAt { get; set; }

    public ProgramParticipation Participation { get; set; } = null!;

    public ICollection<WorkPlanDay> Days { get; set; }
        = new List<WorkPlanDay>();
}