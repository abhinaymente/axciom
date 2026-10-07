using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Activity
    {
        public int ActivityId { get; set; }

        [Required(ErrorMessage = "Activity Type is required.")]
        [StringLength(30)]
        [Display(Name = "Activity Type")]
        public string ActivityType { get; set; } = "Call"; // Call, Meeting, Email, Task

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }
        public Lead? Lead { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Completed"; // Pending, Completed
    }
}
