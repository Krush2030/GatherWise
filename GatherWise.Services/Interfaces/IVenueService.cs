using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Services.Interfaces
{
    public interface IVenueService
    {
        Task<IEnumerable<Venue>> GetAllVenuesAsync();
        Task<Venue?> GetVenueByIdAsync(int id);
        Task<Venue> CreateVenueAsync(Venue venue);
        Task UpdateVenueAsync(Venue venue);
        Task DeleteVenueAsync(int id);
    }
}