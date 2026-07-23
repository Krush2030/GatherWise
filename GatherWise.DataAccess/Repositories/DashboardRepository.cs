using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;
using GatherWise.Domain.Interfaces;

namespace GatherWise.DataAccess.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalSpentByHostAsync(string hostId)
        {
            return await _context.Set<Payment>()
                .Where(p => p.Booking.EventHostId == hostId && p.Status == PaymentStatus.FullyPaid)
                .SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingReservationsByHostAsync(string hostId, int count)
        {
            return await _context.Set<Booking>()
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .Where(b => b.EventHostId == hostId
                            && b.Status == BookingStatus.Approved
                            && b.Slot.Date >= DateTime.UtcNow.Date)
                .OrderBy(b => b.Slot.Date)
                .ThenBy(b => b.Slot.StartTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalEarningsByOwnerAsync(string ownerId)
        {
            return await _context.Set<Payment>()
                .Where(p => p.Booking.Venue.OwnerId == ownerId && p.Status == PaymentStatus.FullyPaid)
                .SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<Booking>> GetIncomingPendingRequestsByOwnerAsync(string ownerId)
        {
            // Define the cutoff time (1 hour ago from current UTC execution time)
            var thresholdTime = DateTime.UtcNow.AddHours(-1);

            // 1. Fetch expired requests that are still marked as PendingApproval
            var expiredRequests = await _context.Bookings // Can use _context.Bookings or _context.Set<Booking>()
                .Include(b => b.Slot)
                .Where(b => b.Venue.OwnerId == ownerId
                            && b.Status == BookingStatus.PendingApproval
                            && b.CreatedAt <= thresholdTime)
                .ToListAsync();

            if (expiredRequests.Any())
            {
                foreach (var request in expiredRequests)
                {
                    // Transition the stale request out of the active flow
                    request.Status = BookingStatus.Cancelled; // Or BookingStatus.Rejected depending on your Enums

                    // CRITICAL: Reopen the venue slot so other clients can instantly book it
                    if (request.Slot != null)
                    {
                        request.Slot.IsBooked = false;
                    }
                }

                // Persist the changes to the database
                await _context.SaveChangesAsync();
            }

            // 2. Return only the unexpired pending requests to the dashboard UI
            return await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .Where(b => b.Venue.OwnerId == ownerId
                            && b.Status == BookingStatus.PendingApproval)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<(int BookedCount, int TotalCount)> GetSlotCountsByOwnerAsync(string ownerId)
        {
            var totalCount = await _context.Set<Slot>().CountAsync(s => s.Venue.OwnerId == ownerId);
            if (totalCount == 0) return (0, 0);

            var bookedCount = await _context.Set<Slot>().CountAsync(s => s.Venue.OwnerId == ownerId && s.IsBooked);
            return (bookedCount, totalCount);
        }

        public async Task<IEnumerable<string>> GetPopularTimeSlotsByOwnerAsync(string ownerId, int count)
        {
            return await _context.Set<Booking>()
                .Where(b => b.Venue.OwnerId == ownerId && b.Status == BookingStatus.Approved)
                .GroupBy(b => new { b.Slot.StartTime, b.Slot.EndTime })
                .OrderByDescending(g => g.Count())
                .Select(g => $"{g.Key.StartTime:hh\\:mm} - {g.Key.EndTime:hh\\:mm} ({g.Count()} bookings)")
                .Take(count)
                .ToListAsync();
        }
    }
}