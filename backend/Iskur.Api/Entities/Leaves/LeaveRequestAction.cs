using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Leaves;

public class LeaveRequestAction : BaseEntity
{
    public Guid LeaveRequestId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? PreviousValues { get; set; }

    public string NewValues { get; set; } = string.Empty;

    public Guid PerformedById { get; set; }

    public string? Reason { get; set; }
}