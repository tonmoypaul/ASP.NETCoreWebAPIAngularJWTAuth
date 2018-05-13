using System.ComponentModel.DataAnnotations;

namespace ASP.NETCoreWebAPIAngularJWTAuth.Dtos
{
    public class ApplicationUserDto
    {
        public string Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }   
             
        public bool IsActive { get; set; }
    }
}