using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GatherWise.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // You can add more custom fields here later (e.g., ProfilePicture, Hometown)
    }
}