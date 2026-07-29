using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class AdminOwnerChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserReportId { get; set; }

        [ForeignKey("UserReportId")]
        public virtual UserReport UserReport { get; set; }

        [Required]
        public string SenderId { get; set; } // Can be Admin ID or Venue Owner ID

        [Required]
        [StringLength(2000)]
        public string MessageText { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}