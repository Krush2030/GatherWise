using System.ComponentModel.DataAnnotations;

namespace GatherWise.Domain.ViewModels
{
    public class UserProfileViewModel
    {
        // Core Identity Properties (Read-Only in View)
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;

        // Editable Common Properties
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        // --- Role Specific Optional Fields ---

        // Vendor Fields
        public string? BusinessName { get; set; }
        public string? ContactName { get; set; }

        // Venue Owner Fields
        public string? OwnerNotes { get; set; } // Any custom field your app uses for venue listings management
    }
}