using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class FollowUp
    {
        public int FollowUpId { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }
        public Lead? Lead { get; set; }

        [Required(ErrorMessage = "Follow-up date is required.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Follow-Up Date")]
        public DateTime FollowUpDate { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [StringLength(30)]
        [Display(Name = "Follow-Up Type")]
        public string FollowUpType { get; set; } = "Call"; // Call, Meeting, Email, Task

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Planned"; // Planned, Completed, Missed, Cancelled

        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
