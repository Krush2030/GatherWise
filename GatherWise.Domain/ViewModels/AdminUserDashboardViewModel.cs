using System.Collections.Generic;

namespace GatherWise.Domain.ViewModels
{
    public class AdminUserDashboardViewModel
    {
        public List<UserDisplayInfo> VenueOwners { get; set; } = new List<UserDisplayInfo>();
        public List<UserDisplayInfo> EventHosts { get; set; } = new List<UserDisplayInfo>();
        public List<UserDisplayInfo> Vendors { get; set; } = new List<UserDisplayInfo>();
    }

    public class UserDisplayInfo
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true; // Useful if you plan to add block/unblock features later
    }
}