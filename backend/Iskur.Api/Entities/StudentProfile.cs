namespace Iskur.Api.Entities;

public class StudentProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;

    public Guid EducationDepartmentId { get; set; }
    public int UniversityRegistrationYear { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Department EducationDepartment { get; set; } = null!;

    public ICollection<ProgramParticipation> Participations { get; set; }
        = new List<ProgramParticipation>();
}