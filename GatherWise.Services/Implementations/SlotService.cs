using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;

        public SlotService(ISlotRepository slotRepository)
        {
            _slotRepository = slotRepository;
        }

        public async Task<IEnumerable<Slot>> GetAllSlotsAsync()
        {
            return await _slotRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Slot>> GetSlotsByVenueIdAsync(int venueId)
        {
            return await _slotRepository.GetByVenueIdAsync(venueId);
        }

        public async Task<IEnumerable<Slot>> GetAvailableSlotsByVenueAndDateAsync(int venueId, DateTime date)
        {
            return await _slotRepository.GetAvailableSlotsAsync(venueId, date);
        }

        public async Task<Slot?> GetSlotByIdAsync(int id)
        {
            return await _slotRepository.GetByIdAsync(id);
        }

        public async Task<Slot> CreateSlotAsync(Slot slot)
        {
            return await _slotRepository.AddAsync(slot);
        }

        public async Task UpdateSlotAsync(Slot slot)
        {
            await _slotRepository.UpdateAsync(slot);
        }

        public async Task DeleteSlotAsync(int id)
        {
            var slot = await _slotRepository.GetByIdAsync(id);
            if (slot != null)
            {
                await _slotRepository.DeleteAsync(slot);
            }
        }
    }
}