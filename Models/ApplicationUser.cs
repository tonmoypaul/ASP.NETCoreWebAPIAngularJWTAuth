using Microsoft.AspNetCore.Identity;

namespace ASP.NETCoreWebAPIAngularJWTAuth.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
    }
}