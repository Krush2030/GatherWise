using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;

namespace GatherWise.DataAccess.Repositories
{
    public class VenueRepository : IVenueRepository
    {
        private readonly ApplicationDbContext _context;

        public VenueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venue>> GetAllAsync()
        {
            return await _context.Venues.Include(v => v.Images).ToListAsync();
        }

        public async Task<Venue?> GetByIdAsync(int id)
        {
            return await _context.Venues.Include(v => v.Images).FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Venue> AddAsync(Venue venue)
        {
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return venue;
        }

        public async Task UpdateAsync(Venue venue)
        {
            _context.Venues.Update(venue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Venue venue)
        {
            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
        }
    }
}