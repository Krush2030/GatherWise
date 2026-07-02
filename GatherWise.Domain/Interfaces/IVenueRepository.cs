using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.Interfaces
{
    public interface IVenueRepository
    {
        Task<IEnumerable<Venue>> GetAllAsync();
        Task<Venue?> GetByIdAsync(int id);
        Task<Venue> AddAsync(Venue venue);
        Task UpdateAsync(Venue venue);
        Task DeleteAsync(Venue venue);
    }
}