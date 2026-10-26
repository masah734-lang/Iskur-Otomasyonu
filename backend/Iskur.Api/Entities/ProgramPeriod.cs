namespace Iskur.Api.Entities;

public class ProgramPeriod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ProgramCode { get; set; } = string.Empty;

    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProgramParticipation> Participations { get; set; }
        = new List<ProgramParticipation>();
}