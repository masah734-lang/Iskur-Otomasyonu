using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Organization;

public class Department : BaseEntity
{
    public Guid FacultyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Faculty Faculty { get; set; } = null!;

    public ICollection<WorkUnit> WorkUnits { get; set; }
        = new List<WorkUnit>();
}