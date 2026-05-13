using Microsoft.AspNetCore.Identity;

namespace LibraryMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = "Member";
    }
}