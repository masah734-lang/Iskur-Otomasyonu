namespace Iskur.Api.Entities;

public class Faculty : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; }
        = new List<Department>();
}