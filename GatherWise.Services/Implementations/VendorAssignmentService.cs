using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class VendorAssignmentService : IVendorAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public VendorAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VendorAssignment?> GetAssignmentByIdAsync(int id)
        {
            return await _context.VendorAssignments
                .Include(va => va.Vendor)
                .Include(va => va.Booking)
                .FirstOrDefaultAsync(va => va.Id == id);
        }

        public async Task<IEnumerable<VendorAssignment>> GetAssignmentsByBookingIdAsync(int bookingId)
        {
            return await _context.VendorAssignments
                .Include(va => va.Vendor)
                .Where(va => va.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<IEnumerable<VendorAssignment>> GetAssignmentsByVendorIdAsync(int vendorId)
        {
            return await _context.VendorAssignments
                .Include(va => va.Booking)
                    .ThenInclude(b => b.Venue)
                .Where(va => va.VendorId == vendorId)
                .ToListAsync();
        }

        public async Task<VendorAssignment> AssignVendorToBookingAsync(VendorAssignment assignment)
        {
            _context.VendorAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task UpdateAssignmentDetailsAsync(VendorAssignment assignment)
        {
            // Update only mutable fields like the negotiated final price and instructions
            var existing = await _context.VendorAssignments.FindAsync(assignment.Id);
            if (existing != null)
            {
                existing.SpecialInstructions = assignment.SpecialInstructions;
                existing.FinalAgreedPrice = assignment.FinalAgreedPrice;

                _context.VendorAssignments.Update(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveVendorFromBookingAsync(int id)
        {
            var assignment = await _context.VendorAssignments.FindAsync(id);
            if (assignment != null)
            {
                _context.VendorAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }
    }
}