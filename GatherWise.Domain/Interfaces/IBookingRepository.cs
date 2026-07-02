using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<IEnumerable<Booking>> GetByHostIdAsync(string hostId);
        Task<IEnumerable<Booking>> GetByOwnerIdAsync(string ownerId);
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking?> GetWithDetailsByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task SaveChangesAsync();

        // Transaction support for atomic workflows
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task AddPaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByBookingIdAsync(int bookingId);
        Task<Slot?> GetSlotByIdAsync(int slotId);
    }
}