using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Customer Name required")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone required")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedDate { get; set; }
    }

    public class LeadDto
    {
        public int LeadId { get; set; }
        public string LeadCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lead Name required")]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone required")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        public string? CompanyName { get; set; }
        public string Source { get; set; } = "Website";
        public string Status { get; set; } = "New";
        
        [Range(0, 100000000, ErrorMessage = "Expected Value cannot be negative.")]
        public decimal ExpectedValue { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class OpportunityDto
    {
        public int OpportunityId { get; set; }

        [Required(ErrorMessage = "Opportunity Name required")]
        public string OpportunityName { get; set; } = string.Empty;

        public int? CustomerId { get; set; }
        public int? LeadId { get; set; }

        [Required]
        [Range(0.01, 1000000000.00, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        public string Stage { get; set; } = "Qualification";

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int Probability { get; set; } = 20;

        public DateTime ExpectedCloseDate { get; set; }
        public string Status { get; set; } = "Active";
        public decimal WeightedAmount => Amount * Probability / 100m;
        public DateTime CreatedDate { get; set; }
    }
}
