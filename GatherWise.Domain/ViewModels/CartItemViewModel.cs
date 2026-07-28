namespace GatherWise.Domain.ViewModels
{
    public class CartItemViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string MainPhotoPath { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
    }
}