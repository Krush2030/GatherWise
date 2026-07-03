using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GatherWise.Domain.ViewModels
{
    public class VendorServiceViewModel
    {
        [Required(ErrorMessage = "Service Title is required")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a functional domain category")]
        public string ServiceCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "A direct contact phone line is required")]
        [Phone]
        public string ServicePhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pricing threshold is required")]
        [Range(0.01, 1000000.00)]
        public decimal BasePrice { get; set; }

        [Required(ErrorMessage = "Please scale your rate quote against a person count")]
        [Range(1, 10000, ErrorMessage = "Must apply to at least 1 person")]
        public int PerNumberOfPersons { get; set; }

        [Required(ErrorMessage = "Primary identification banner/logo is required")]
        public IFormFile MainPhoto { get; set; }

        public List<IFormFile> GalleryImages { get; set; } = new();
    }
}