using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GatherWise.Web.Models
{
    public class AssignVendorViewModel
    {
        [Required(ErrorMessage = "Please select a booking.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Please select a vendor.")]
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Please specify the final agreed contract price.")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be a positive value.")]
        public decimal FinalAgreedPrice { get; set; }

        [StringLength(500, ErrorMessage = "Instructions cannot exceed 500 characters.")]
        public string? SpecialInstructions { get; set; }

        // Select lists to feed our drop-downs
        public SelectList? BookingsList { get; set; }
        public SelectList? VendorsList { get; set; }
    }
}