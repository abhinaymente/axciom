using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // LOGIN, FAILED_LOGIN, LOGOUT, CREATE, UPDATE, DELETE, ROLE_CHANGE

        [StringLength(100)]
        [Display(Name = "Entity")]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Record ID")]
        public string RecordId { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        [Display(Name = "Log Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        [Display(Name = "IP Address")]
        public string IpAddress { get; set; } = "127.0.0.1";
    }
}
