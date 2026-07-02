using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;

namespace GatherWise.DataAccess.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Venue)
                        .ThenInclude(v => v.Owner)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.EventHost)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetByHostIdAsync(string hostId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Venue)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .Where(p => p.Booking.EventHostId == hostId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByOwnerIdAsync(string ownerId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Venue)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .Where(p => p.Booking.Venue.OwnerId == ownerId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetTrackedByIdAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}