using System.Collections.Generic;
using System.IO;
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

        public VenueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            return await _context.Venues.Include(v => v.Images).ToListAsync();
        }

        public async Task<Venue?> GetVenueByIdAsync(int id)
        {
            return await _context.Venues.Include(v => v.Images).FirstOrDefaultAsync(v => v.Id == id);
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
            var venue = await _context.Venues.Include(v => v.Images).FirstOrDefaultAsync(v => v.Id == id);
            if (venue != null)
            {
                foreach (var img in venue.Images)
                {
                    // Trims leading '/' from path combine steps
                    string cleanPath = img.ImagePath.TrimStart('/');
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleanPath);

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }
        }
    }
}