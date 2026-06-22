using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class VendorAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required]
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500, ErrorMessage = "Special instructions cannot exceed 500 characters")]
        public string? SpecialInstructions { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalAgreedPrice { get; set; } // Can vary from the vendor's base price depending on custom scales

        // Navigation Properties
        public Booking? Booking { get; set; }
        public Vendor? Vendor { get; set; }
    }
}