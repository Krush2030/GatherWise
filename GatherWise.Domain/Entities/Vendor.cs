using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class Vendor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vendor company name is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service category is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string ServiceCategory { get; set; } = string.Empty; // e.g., Catering, Photography, Decoration

        [Required(ErrorMessage = "Contact person name is required")]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address formatting")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base service price is required")]
        [Range(0.00, 500000.00, ErrorMessage = "Invalid pricing amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasePrice { get; set; }

        public bool IsActive { get; set; } = true;
    }
}