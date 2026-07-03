using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class Vendor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vendor company name is required")]
        [StringLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person name is required")]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; }

        // Navigation property supporting multiple services
        public ICollection<VendorService> Services { get; set; } = new List<VendorService>();
    }
}