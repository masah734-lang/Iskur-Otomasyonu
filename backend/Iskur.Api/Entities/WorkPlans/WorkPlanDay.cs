using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Identity;

namespace Iskur.Api.Entities.WorkPlans;

public class WorkPlanDay : BaseEntity
{
    public Guid MonthlyWorkPlanId { get; set; }

    public DateOnly WorkDate { get; set; }

    public WorkType WorkType { get; set; }

    public string? Explanation { get; set; }

    public DecisionStatus Decision { get; set; }
        = DecisionStatus.Draft;

    public Guid? DecidedById { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public string? DecisionReason { get; set; }

    public int VersionNumber { get; set; } = 1;

    public MonthlyWorkPlan MonthlyWorkPlan { get; set; } = null!;

    public ApplicationUser? DecidedBy { get; set; }

    public ICollection<WorkSession> Sessions { get; set; }
        = new List<WorkSession>();

    public ICollection<PlanDayAction> Actions { get; set; }
        = new List<PlanDayAction>();
}