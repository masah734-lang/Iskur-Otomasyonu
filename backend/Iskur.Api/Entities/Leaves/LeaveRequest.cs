using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Leaves;

public class LeaveRequest : BaseEntity
{
    public Guid ParticipationId { get; set; }

    public Guid LeaveTypeId { get; set; }

    // Başlangıç ve bitiş günleri başvuruya dahildir.
    public DateOnly StartsOn { get; set; }

    public DateOnly EndsOn { get; set; }

    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public DateTimeOffset SubmittedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public Guid? DecidedById { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public string? DecisionReason { get; set; }
}