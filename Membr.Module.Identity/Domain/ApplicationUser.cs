using Microsoft.AspNetCore.Identity;

namespace Membr.Module.Identity.Domain;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = null!;
}
