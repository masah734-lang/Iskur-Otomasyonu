using Microsoft.AspNetCore.Identity;

namespace Iskur.Api.Entities;

public class AuthorityAssignment : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public Guid? FacultyId { get; set; }
    public Guid? DepartmentId { get; set; }

    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }

    public Guid AssignedById { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public IdentityRole<Guid> Role { get; set; } = null!;

    public Faculty? Faculty { get; set; }
    public Department? Department { get; set; }

    public ApplicationUser AssignedBy { get; set; } = null!;
}