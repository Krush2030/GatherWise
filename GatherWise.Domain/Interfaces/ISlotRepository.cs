using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.Interfaces
{
    public interface ISlotRepository
    {
        Task<IEnumerable<Slot>> GetAllAsync();
        Task<IEnumerable<Slot>> GetByVenueIdAsync(int venueId);
        Task<IEnumerable<Slot>> GetAvailableSlotsAsync(int venueId, DateTime date);
        Task<Slot?> GetByIdAsync(int id);
        Task<Slot> AddAsync(Slot slot);
        Task UpdateAsync(Slot slot);
        Task DeleteAsync(Slot slot);
    }
}