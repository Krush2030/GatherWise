using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Venue name is required")]
        [StringLength(100, ErrorMessage = "Venue name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(255, ErrorMessage = "Location address cannot exceed 255 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 50000, ErrorMessage = "Capacity must be between 1 and 50,000 guests")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Price per slot is required")]
        [Range(0.00, 1000000.00, ErrorMessage = "Invalid price amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerSlot { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        // Foreign Key Mapping to identity user model
        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; }

        public ICollection<VenueImage> Images { get; set; } = new List<VenueImage>();
    }
}