using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;

namespace GatherWise.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<IEnumerable<Booking>> GetBookingsByHostIdAsync(string hostId);
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<Booking> CreateBookingAsync(Booking booking);
        //Task CancelBookingAsync(int id);
        Task UpdateBookingStatusAsync(int id, BookingStatus status);

        Task<IEnumerable<Booking>> GetBookingsByOwnerIdAsync(string ownerId);

        Task<bool> CancelBookingAsync(int bookingId);
    }
}