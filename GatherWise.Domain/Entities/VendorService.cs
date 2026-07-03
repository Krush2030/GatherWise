using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class VendorService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VendorId { get; set; }

        [ForeignKey("VendorId")]
        public Vendor? Vendor { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100)]
        public string ServiceName { get; set; } = string.Empty; // e.g., Premium Wedding Catering

        [Required(ErrorMessage = "Service category is required")]
        [StringLength(50)]
        public string ServiceCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service telephone line is required")]
        [Phone]
        [StringLength(15)]
        public string ServicePhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base service price is required")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Capacity scale metric definition is required")]
        public int PerNumberOfPersons { get; set; } // e.g., 50 (Translates pricing logic to: $BasePrice per 50 people)

        // File Path Storage Elements
        [Required(ErrorMessage = "Main display banner/logo photo is required")]
        public string MainPhotoPath { get; set; } = string.Empty;

        // Supporting image assets collection
        public ICollection<VendorServiceImage> GalleryImages { get; set; } = new List<VendorServiceImage>();
    }
}