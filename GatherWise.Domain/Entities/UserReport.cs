using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class UserReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ReporterId { get; set; }

        [Required]
        public string ReportedUserId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        [StringLength(1000)]
        public string? AdminReply { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // Optional Navigation Properties depending on how you structure ApplicationUser
        // [ForeignKey("ReporterId")]
        // public virtual ApplicationUser Reporter { get; set; }
        // [ForeignKey("ReportedUserId")]
        // public virtual ApplicationUser ReportedUser { get; set; }
    }
}