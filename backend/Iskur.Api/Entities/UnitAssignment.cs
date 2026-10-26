namespace Iskur.Api.Entities;

public class UnitAssignment : BaseEntity
{
    public Guid ParticipationId { get; set; }
    public Guid WorkUnitId { get; set; }

    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }

    public Guid AssignedById { get; set; }
    public string? Reason { get; set; }

    public ProgramParticipation Participation { get; set; } = null!;
    public WorkUnit WorkUnit { get; set; } = null!;
    public ApplicationUser AssignedBy { get; set; } = null!;
}