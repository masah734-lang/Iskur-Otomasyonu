using Iskur.Api.Entities.Common;

namespace Iskur.Api.Entities.Organization;

public class WorkUnit : BaseEntity
{
    public Guid DepartmentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Department Department { get; set; } = null!;
}