using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Slot)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Venue)
                        .ThenInclude(v => v.Owner) // Loads the Venue Owner ApplicationUser profile
                .Include(p => p.Booking)
                    .ThenInclude(b => b.EventHost)  // Loads the Event Host ApplicationUser profile
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByHostIdAsync(string hostId)
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

        public async Task<IEnumerable<Payment>> GetPaymentsByOwnerIdAsync(string ownerId)
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

        public async Task<bool> ProcessPaymentAsync(int paymentId, string paymentMethod)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null || payment.Status == PaymentStatus.FullyPaid)
            {
                return false;
            }

            // Update status and complete transaction metadata
            payment.Status = PaymentStatus.FullyPaid;
            payment.PaymentMethod = paymentMethod;
            payment.PaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}