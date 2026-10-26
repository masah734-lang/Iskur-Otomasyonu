using Microsoft.AspNetCore.Identity;

namespace Iskur.Api.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsAccountOpen { get; set; } = true;

    public bool IsFirstLoginCompleted { get; set; }

    public byte[] RowVersion { get; set; } = [];
}