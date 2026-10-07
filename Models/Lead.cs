using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models
{
    public class Lead
    {
        public int LeadId { get; set; }

        [Display(Name = "Lead Code")]
        public string LeadCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a valid lead name.")]
        [StringLength(100)]
        [Display(Name = "Lead Name")]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a valid email address.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a valid phone number.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Company Name")]
        [StringLength(150)]
        public string? CompanyName { get; set; }

        [Required]
        [StringLength(50)]
        public string Source { get; set; } = "Website"; // Website, Referral, Cold Call, Social Media, Trade Show, Other

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "New"; // New, Contacted, Qualified, Lost, Converted

        [Range(0, 100000000, ErrorMessage = "Expected Value cannot be negative.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Expected Value")]
        public decimal ExpectedValue { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; } = string.Empty;

        public int? ConvertedCustomerId { get; set; }
        public Customer? ConvertedCustomer { get; set; }

        public int? ConvertedOpportunityId { get; set; }
        public Opportunity? ConvertedOpportunity { get; set; }
    }
}
