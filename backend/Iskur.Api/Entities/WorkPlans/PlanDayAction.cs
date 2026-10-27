using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Identity;

namespace Iskur.Api.Entities.WorkPlans;

public class PlanDayAction : BaseEntity
{
    public Guid WorkPlanDayId { get; set; }

    public int VersionNumber { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? PreviousValues { get; set; }

    public string NewValues { get; set; } = string.Empty;

    public Guid PerformedById { get; set; }

    public string? Reason { get; set; }

    public WorkPlanDay WorkPlanDay { get; set; } = null!;

    public ApplicationUser PerformedBy { get; set; } = null!;
}