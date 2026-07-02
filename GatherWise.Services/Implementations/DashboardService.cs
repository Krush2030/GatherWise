using System;
using System.Collections.Generic;
using System.Linq; // Ensure Linq is included for .ToList()
using System.Threading.Tasks;
using GatherWise.Domain.Interfaces;
using GatherWise.Domain.ViewModels;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardViewModel> GetHostDashboardDataAsync(string hostId)
        {
            var model = new DashboardViewModel
            {
                ActiveUserRole = "Event Host",
                TotalSpent = await _dashboardRepository.GetTotalSpentByHostAsync(hostId),
                // Materialize the IEnumerable into a concrete List
                UpcomingReservations = (await _dashboardRepository.GetUpcomingReservationsByHostAsync(hostId, 5)).ToList()
            };

            return model;
        }

        public async Task<DashboardViewModel> GetOwnerDashboardDataAsync(string ownerId)
        {
            var model = new DashboardViewModel
            {
                ActiveUserRole = "Venue Owner",
                TotalEarnings = await _dashboardRepository.GetTotalEarningsByOwnerAsync(ownerId),
                // Materialize the IEnumerable into a concrete List
                IncomingRequests = (await _dashboardRepository.GetIncomingPendingRequestsByOwnerAsync(ownerId)).ToList()
            };

            // Calculate occupancy metrics
            var (bookedCount, totalCount) = await _dashboardRepository.GetSlotCountsByOwnerAsync(ownerId);
            model.VenueOccupancyRate = totalCount > 0 ? ((double)bookedCount / totalCount) * 100 : 0.0;

            // Fetch aggregate group tracking lines
            model.PopularTimeSlots = (await _dashboardRepository.GetPopularTimeSlotsByOwnerAsync(ownerId, 3)).ToList();

            return model;
        }
    }
}