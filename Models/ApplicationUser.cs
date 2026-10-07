using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [PersonalData]
        public string Department { get; set; } = "Sales";

        [PersonalData]
        public bool IsActive { get; set; } = true;

        [PersonalData]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
