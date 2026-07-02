using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;

namespace GatherWise.DataAccess.Repositories
{
    public class SlotRepository : ISlotRepository
    {
        private readonly ApplicationDbContext _context;

        public SlotRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Slot>> GetAllAsync()
        {
            return await _context.Slots
                .Include(s => s.Venue)
                .ToListAsync();
        }

        public async Task<IEnumerable<Slot>> GetByVenueIdAsync(int venueId)
        {
            return await _context.Slots
                .Where(s => s.VenueId == venueId)
                .Include(s => s.Venue)
                .ToListAsync();
        }

        public async Task<IEnumerable<Slot>> GetAvailableSlotsAsync(int venueId, DateTime date)
        {
            return await _context.Slots
                .Where(s => s.VenueId == venueId && s.Date.Date == date.Date && !s.IsBooked)
                .Include(s => s.Venue)
                .ToListAsync();
        }

        public async Task<Slot?> GetByIdAsync(int id)
        {
            return await _context.Slots
                .Include(s => s.Venue)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Slot> AddAsync(Slot slot)
        {
            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();
            return slot;
        }

        public async Task UpdateAsync(Slot slot)
        {
            _context.Slots.Update(slot);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Slot slot)
        {
            _context.Slots.Remove(slot);
            await _context.SaveChangesAsync();
        }
    }
}