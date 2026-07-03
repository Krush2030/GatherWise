using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class VendorServiceImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VendorServiceId { get; set; }

        [ForeignKey("VendorServiceId")]
        public VendorService? VendorService { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;
    }
}