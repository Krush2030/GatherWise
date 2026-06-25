using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;

namespace GatherWise.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByHostIdAsync(string hostId);
        Task<IEnumerable<Payment>> GetPaymentsByOwnerIdAsync(string ownerId);
        Task<bool> ProcessPaymentAsync(int paymentId, string paymentMethod);
    }
}