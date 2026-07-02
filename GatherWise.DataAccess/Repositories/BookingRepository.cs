using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;

namespace GatherWise.DataAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByHostIdAsync(string hostId)
        {
            return await _context.Bookings
                .Where(b => b.EventHostId == hostId)
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByOwnerIdAsync(string ownerId)
        {
            return await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .Where(b => b.Venue.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task<Booking?> GetWithDetailsByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        public async Task<Slot?> GetSlotByIdAsync(int slotId)
        {
            return await _context.Slots.FindAsync(slotId);
        }
    }
}