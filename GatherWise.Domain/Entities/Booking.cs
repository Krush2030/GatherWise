using System;
using System.Collections.Generic;
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
        public string EventHostId { get; set; } = string.Empty;

        // Foreign Key Mapping to identity user model
        [ForeignKey("EventHostId")]
        public ApplicationUser? EventHost { get; set; }

        [Required(ErrorMessage = "Venue selection is required")]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Time slot selection is required")]
        [ForeignKey("Slot")]
        public int SlotId { get; set; }

        // Tracks when the Booking Request was generated
        [Required(ErrorMessage = "Booking date record is required")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ADDED: Tracks the exact moment the Venue Owner clicked "Approve"
        // This establishes the starting line for the Host's 1-hour payment countdown window
        [DataType(DataType.DateTime)]
        public DateTime? ApprovedAt { get; set; }

        [Required(ErrorMessage = "Estimated guest count is required")]
        [Range(1, 50000, ErrorMessage = "Guest count must be at least 1 and within venue capacity limits")]
        public int EstimatedGuests { get; set; }

        [Required(ErrorMessage = "Total price calculation is required")]
        [Range(0.00, 5000000.00, ErrorMessage = "Invalid price calculation")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        // UPDATED: Default state changed to PendingApproval
        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.PendingApproval;

        // Navigation Properties for EF Core Joins
        public ICollection<BookingService> BookedServices { get; set; } = new List<BookingService>();
        public Venue? Venue { get; set; }
        public Slot? Slot { get; set; }
    }
}