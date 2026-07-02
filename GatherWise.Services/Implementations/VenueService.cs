using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _venueRepository;

        public VenueService(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            return await _venueRepository.GetAllAsync();
        }

        public async Task<Venue?> GetVenueByIdAsync(int id)
        {
            return await _venueRepository.GetByIdAsync(id);
        }

        public async Task<Venue> CreateVenueAsync(Venue venue)
        {
            return await _venueRepository.AddAsync(venue);
        }

        public async Task UpdateVenueAsync(Venue venue)
        {
            await _venueRepository.UpdateAsync(venue);
        }

        public async Task DeleteVenueAsync(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id);
            if (venue != null)
            {
                // Business logic: Remove file attachments from local disk storage 
                foreach (var img in venue.Images)
                {
                    string cleanPath = img.ImagePath.TrimStart('/');
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleanPath);

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }

                // Data access persistence call
                await _venueRepository.DeleteAsync(venue);
            }
        }
    }
}