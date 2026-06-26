using System.Collections.Generic;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.ViewModels
{
    public class DashboardViewModel
    {
        // Common Role Attributes
        public string ActiveUserRole { get; set; } = string.Empty;

        // Customer (Event Host) Specific Metrics
        public decimal TotalSpent { get; set; }
        public List<Booking> UpcomingReservations { get; set; } = new();

        // Venue Owner Specific Metrics
        public decimal TotalEarnings { get; set; }
        public double VenueOccupancyRate { get; set; }
        public List<string> PopularTimeSlots { get; set; } = new();
        public List<Booking> IncomingRequests { get; set; } = new();
    }
}