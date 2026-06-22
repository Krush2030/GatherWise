using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GatherWise.Domain.Enums;

namespace GatherWise.Domain.Entities
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Payment must be linked to a valid booking")]
        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Transaction amount is required")]
        [Range(0.01, 1000000.00, ErrorMessage = "Invalid transaction amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required(ErrorMessage = "Payment method description is required")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "CreditCard"; // e.g., CreditCard, UPI, NetBanking

        [StringLength(100)]
        public string? TransactionId { get; set; } // For external payment gateway logs

        // Navigation Property
        public Booking? Booking { get; set; }
    }
}