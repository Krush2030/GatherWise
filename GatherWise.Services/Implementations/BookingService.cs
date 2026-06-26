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

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Status == BookingStatus.Cancelled)
                return false;

            // 1. Change Booking Status
            booking.Status = BookingStatus.Cancelled;

            // 2. Free up the slot timeline execution window
            if (booking.Slot != null)
            {
                booking.Slot.IsBooked = false;
            }

            // 3. Handle the attached financial ledger state
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment != null)
            {
                // Adjust this mapping based on your GatherWise.Domain.Enums.PaymentStatus values
                payment.Status = PaymentStatus.Refunded;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task UpdateBookingStatusAsync(int id, BookingStatus status)
        {
            // Load with Slot inclusion
            var booking = await _context.Bookings
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking != null)
            {
                booking.Status = status;

                // If it gets confirmed, explicitly fetch and lock down the slot row itself
                if (status == BookingStatus.Confirmed && booking.Slot != null)
                {
                    booking.Slot.IsBooked = true;
                    _context.Slots.Update(booking.Slot); // Force EF Core to mark the Slot table row as dirty/updated
                }

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