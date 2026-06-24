using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByHostIdAsync(string hostId)
        {
            return await _context.Bookings
                .Where(b => b.EventHostId == hostId)
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Fetch the selected slot to check availability and mark it reserved
                var slot = await _context.Slots.FindAsync(booking.SlotId);
                if (slot == null || slot.IsBooked)
                {
                    throw new InvalidOperationException("The requested slot is either invalid or already reserved.");
                }

                slot.IsBooked = true;

                // 2. Add the Booking
                booking.CreatedAt = DateTime.UtcNow;
                booking.Status = BookingStatus.Pending;
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync(); // Saves to generate booking.Id

                // 3. Automatically initialize an upfront invoice tracking record for this booking
                var initialInvoice = new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending,
                    PaymentMethod = "CreditCard"
                };
                _context.Payments.Add(initialInvoice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return booking;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelBookingAsync(int id)
        {
            // Make sure to include the underlying Slot navigation entity!
            var booking = await _context.Bookings
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking != null)
            {
                // 1. Change the business status of the request
                booking.Status = BookingStatus.Cancelled;

                // 2. RELEASE THE SLOT! Reset IsBooked back to false so it can be re-booked
                if (booking.Slot != null)
                {
                    booking.Slot.IsBooked = false;
                }

                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateBookingStatusAsync(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Booking>> GetBookingsByOwnerIdAsync(string ownerId)
        {
            return await _context.Bookings
            .Include(b => b.Venue)
            .Include(b => b.Slot)
            .Where(b => b.Venue.OwnerId == ownerId)
            .ToListAsync();
        }
    }
}