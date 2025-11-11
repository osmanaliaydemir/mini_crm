using Microsoft.AspNetCore.Identity;

namespace CRM.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string? Locale { get; set; }
}

