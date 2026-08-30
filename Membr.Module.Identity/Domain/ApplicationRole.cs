using Microsoft.AspNetCore.Identity;

namespace Membr.Module.Identity.Domain;

public class ApplicationRole : IdentityRole
{
    public bool Enabled { get; set; }
}
