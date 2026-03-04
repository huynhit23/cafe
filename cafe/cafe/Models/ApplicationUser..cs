using Microsoft.AspNetCore.Identity;

namespace cafe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
