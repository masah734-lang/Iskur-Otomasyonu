namespace Iskur.Api.Entities;

public class AdvisorAssignment : BaseEntity
{
    public Guid ParticipationId { get; set; }
    public Guid AdvisorId { get; set; }

    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }

    public Guid AssignedById { get; set; }
    public string? Reason { get; set; }

    public ProgramParticipation Participation { get; set; } = null!;
    public ApplicationUser Advisor { get; set; } = null!;
    public ApplicationUser AssignedBy { get; set; } = null!;
}