using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class SlotService : ISlotService
    {
        private readonly ApplicationDbContext _context;

        public SlotService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Slot>> GetAllSlotsAsync()
        {
            return await _context.Slots
                .Include(s => s.Venue)
                .ToListAsync();
        }

        public async Task<IEnumerable<Slot>> GetSlotsByVenueIdAsync(int venueId)
        {
            return await _context.Slots
                .Where(s => s.VenueId == venueId)
                .Include(s => s.Venue)
                .ToListAsync();
        }

        public async Task<IEnumerable<Slot>> GetAvailableSlotsByVenueAndDateAsync(int venueId, DateTime date)
        {
            return await _context.Slots
                .Where(s => s.VenueId == venueId && s.Date.Date == date.Date && !s.IsBooked)
                .Include(s => s.Venue)
                .ToListAsync();
        }

        public async Task<Slot?> GetSlotByIdAsync(int id)
        {
            return await _context.Slots
                .Include(s => s.Venue)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Slot> CreateSlotAsync(Slot slot)
        {
            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }

        public async Task UpdateSlotAsync(Slot slot)
        {
            _context.Slots.Update(slot);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSlotAsync(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot != null)
            {
                _context.Slots.Remove(slot);
                await _context.SaveChangesAsync();
            }
        }
    }
}