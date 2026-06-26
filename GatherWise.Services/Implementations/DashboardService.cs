using System;
using System.Linq;
using System.Threading.Tasks;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;
using GatherWise.Domain.ViewModels;
using GatherWise.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GatherWise.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        // Changed from DbContext to ApplicationDbContext
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetHostDashboardDataAsync(string hostId)
        {
            var model = new DashboardViewModel
            {
                ActiveUserRole = "Event Host"
            };

            // 1. Calculate overall event expenditure outlays from paid invoices
            model.TotalSpent = await _context.Set<Payment>()
                .Where(p => p.Booking.EventHostId == hostId && p.Status == PaymentStatus.FullyPaid)
                .SumAsync(p => p.Amount);

            // 2. Fetch upcoming chronological booking confirmation lines
            model.UpcomingReservations = await _context.Set<Booking>()
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .Where(b => b.EventHostId == hostId && b.Status == BookingStatus.Confirmed && b.Slot.Date >= DateTime.UtcNow.Date)
                .OrderBy(b => b.Slot.Date)
                .ThenBy(b => b.Slot.StartTime)
                .Take(5)
                .ToListAsync();

            return model;
        }

        public async Task<DashboardViewModel> GetOwnerDashboardDataAsync(string ownerId)
        {
            var model = new DashboardViewModel
            {
                ActiveUserRole = "Venue Owner"
            };

            // 1. Calculate Total Gross Earnings from properties owned by this operator
            model.TotalEarnings = await _context.Set<Payment>()
                .Where(p => p.Booking.Venue.OwnerId == ownerId && p.Status == PaymentStatus.FullyPaid)
                .SumAsync(p => p.Amount);

            // 2. Fetch Incoming Requests that require confirmation or processing
            model.IncomingRequests = await _context.Set<Booking>()
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .Where(b => b.Venue.OwnerId == ownerId && b.Status == BookingStatus.Pending)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // 3. Compute Venue Occupancy Rate Metric (Booked Slots / Total Slots configured)
            var totalOwnerSlotsCount = await _context.Set<Slot>()
                .CountAsync(s => s.Venue.OwnerId == ownerId);

            if (totalOwnerSlotsCount > 0)
            {
                var bookedOwnerSlotsCount = await _context.Set<Slot>()
                    .CountAsync(s => s.Venue.OwnerId == ownerId && s.IsBooked);

                // Percentage calculation logic metrics
                model.VenueOccupancyRate = ((double)bookedOwnerSlotsCount / totalOwnerSlotsCount) * 100;
            }
            else
            {
                model.VenueOccupancyRate = 0.0;
            }

            // 4. Extract most popular time slot strings using GroupBy aggregation mappings
            model.PopularTimeSlots = await _context.Set<Booking>()
                .Where(b => b.Venue.OwnerId == ownerId && b.Status == BookingStatus.Confirmed)
                .GroupBy(b => new { b.Slot.StartTime, b.Slot.EndTime })
                .OrderByDescending(g => g.Count())
                .Select(g => $"{g.Key.StartTime:hh\\:mm} - {g.Key.EndTime:hh\\:mm} ({g.Count()} bookings)")
                .Take(3)
                .ToListAsync();

            return model;
        }
    }
}