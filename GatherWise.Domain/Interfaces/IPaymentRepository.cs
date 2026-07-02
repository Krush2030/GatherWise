using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> GetByHostIdAsync(string hostId);
        Task<IEnumerable<Payment>> GetByOwnerIdAsync(string ownerId);
        Task<Payment?> GetTrackedByIdAsync(int id); // Used for tracking modifications
        Task SaveChangesAsync();
    }
}