using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class VenueService : IVenueService
    {
        private readonly ApplicationDbContext _context;

        // Inject the ApplicationDbContext using Dependency Injection
        public VenueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            return await _context.Venues.ToListAsync();
        }

        public async Task<Venue?> GetVenueByIdAsync(int id)
        {
            return await _context.Venues.FindAsync(id);
        }

        public async Task<Venue> CreateVenueAsync(Venue venue)
        {
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return venue;
        }

        public async Task UpdateVenueAsync(Venue venue)
        {
            _context.Venues.Update(venue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVenueAsync(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }
        }
    }
}