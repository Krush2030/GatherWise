using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GatherWise.Domain.Enums;

namespace GatherWise.Domain.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer identification is required")]
        public string EventHostId { get; set; } = string.Empty; // Will link to ASP.NET Core Identity User ID string

        [Required(ErrorMessage = "Venue selection is required")]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Time slot selection is required")]
        [ForeignKey("Slot")]
        public int SlotId { get; set; }

        [Required(ErrorMessage = "Booking date record is required")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Estimated guest count is required")]
        [Range(1, 50000, ErrorMessage = "Guest count must be at least 1 and within venue capacity limits")]
        public int EstimatedGuests { get; set; }

        [Required(ErrorMessage = "Total price calculation is required")]
        [Range(0.00, 5000000.00, ErrorMessage = "Invalid price calculation")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Navigation Properties for EF Core Joins
        public Venue? Venue { get; set; }
        public Slot? Slot { get; set; }
    }
}