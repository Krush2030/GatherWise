using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;
using GatherWise.Domain.Interfaces;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByHostIdAsync(string hostId)
        {
            return await _bookingRepository.GetByHostIdAsync(hostId);
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _bookingRepository.GetWithDetailsByIdAsync(id);
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            using var transaction = await _bookingRepository.BeginTransactionAsync();
            try
            {
                // 1. Check slot availability
                var slot = await _bookingRepository.GetSlotByIdAsync(booking.SlotId);
                if (slot == null || slot.IsBooked)
                {
                    throw new InvalidOperationException("The requested slot is either invalid or already reserved.");
                }

                slot.IsBooked = true;

                // 2. Insert Base Booking record
                booking.CreatedAt = DateTime.UtcNow;
                booking.Status = BookingStatus.Pending;
                await _bookingRepository.AddAsync(booking);
                await _bookingRepository.SaveChangesAsync(); // Commit to generate booking.Id identity increment value

                // 3. Automatically append initial invoice statement entry
                var initialInvoice = new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending,
                    PaymentMethod = "CreditCard"
                };
                await _bookingRepository.AddPaymentAsync(initialInvoice);

                await _bookingRepository.SaveChangesAsync();
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
            var booking = await _bookingRepository.GetWithDetailsByIdAsync(bookingId);
            if (booking == null || booking.Status == BookingStatus.Cancelled)
                return false;

            booking.Status = BookingStatus.Cancelled;

            if (booking.Slot != null)
            {
                booking.Slot.IsBooked = false;
            }

            var payment = await _bookingRepository.GetPaymentByBookingIdAsync(bookingId);
            if (payment != null)
            {
                payment.Status = PaymentStatus.Refunded;
            }

            await _bookingRepository.SaveChangesAsync();
            return true;
        }

        public async Task UpdateBookingStatusAsync(int id, BookingStatus status)
        {
            var booking = await _bookingRepository.GetWithDetailsByIdAsync(id);
            if (booking != null)
            {
                booking.Status = status;

                if (status == BookingStatus.Confirmed && booking.Slot != null)
                {
                    booking.Slot.IsBooked = true;
                }

                await _bookingRepository.UpdateAsync(booking);
                await _bookingRepository.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Booking>> GetBookingsByOwnerIdAsync(string ownerId)
        {
            return await _bookingRepository.GetByOwnerIdAsync(ownerId);
        }
    }
}