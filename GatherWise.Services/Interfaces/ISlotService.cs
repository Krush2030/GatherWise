using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Services.Interfaces
{
    public interface ISlotService
    {
        Task<IEnumerable<Slot>> GetAllSlotsAsync();
        Task<IEnumerable<Slot>> GetSlotsByVenueIdAsync(int venueId);
        Task<IEnumerable<Slot>> GetAvailableSlotsByVenueAndDateAsync(int venueId, DateTime date);
        Task<Slot?> GetSlotByIdAsync(int id);
        Task<Slot> CreateSlotAsync(Slot slot);
        Task UpdateSlotAsync(Slot slot);
        Task DeleteSlotAsync(int id);
    }
}