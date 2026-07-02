using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.Interfaces
{
    public interface IDashboardRepository
    {
        // Host Metrics
        Task<decimal> GetTotalSpentByHostAsync(string hostId);
        Task<IEnumerable<Booking>> GetUpcomingReservationsByHostAsync(string hostId, int count);

        // Owner Metrics
        Task<decimal> GetTotalEarningsByOwnerAsync(string ownerId);
        Task<IEnumerable<Booking>> GetIncomingPendingRequestsByOwnerAsync(string ownerId);
        Task<(int BookedCount, int TotalCount)> GetSlotCountsByOwnerAsync(string ownerId);
        Task<IEnumerable<string>> GetPopularTimeSlotsByOwnerAsync(string ownerId, int count);
    }
}