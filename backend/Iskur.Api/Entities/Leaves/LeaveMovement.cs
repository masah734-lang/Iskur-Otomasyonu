using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Leaves;

public class LeaveMovement : BaseEntity
{
    public Guid ParticipationId { get; set; }

    public Guid? LeaveRequestId { get; set; }

    public Guid? LeavePolicyId { get; set; }

    public LeaveMovementType MovementType { get; set; }

    // Hak ve iade pozitif; kullanım negatif değerle kaydedilir.
    public decimal DayAmount { get; set; }

    public Guid PerformedById { get; set; }

    public string? Description { get; set; }
}