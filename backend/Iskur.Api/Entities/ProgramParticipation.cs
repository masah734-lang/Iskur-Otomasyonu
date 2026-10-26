namespace Iskur.Api.Entities;

public class ProgramParticipation : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid ProgramPeriodId { get; set; }

    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }

    public ParticipationStatus Status { get; set; }
        = ParticipationStatus.Active;

    public string? LeavingReason { get; set; }

    public StudentProfile Student { get; set; } = null!;
    public ProgramPeriod ProgramPeriod { get; set; } = null!;

    public ICollection<UnitAssignment> UnitAssignments { get; set; }
        = new List<UnitAssignment>();

    public ICollection<AdvisorAssignment> AdvisorAssignments { get; set; }
        = new List<AdvisorAssignment>();
}