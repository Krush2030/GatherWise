using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;
using GatherWise.Domain.Interfaces;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByHostIdAsync(string hostId)
        {
            return await _paymentRepository.GetByHostIdAsync(hostId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByOwnerIdAsync(string ownerId)
        {
            return await _paymentRepository.GetByOwnerIdAsync(ownerId);
        }

        public async Task<bool> ProcessPaymentAsync(int paymentId, string paymentMethod)
        {
            var payment = await _paymentRepository.GetTrackedByIdAsync(paymentId);
            if (payment == null || payment.Status == PaymentStatus.FullyPaid)
            {
                return false;
            }

            // Execute business workflow assignments
            payment.Status = PaymentStatus.FullyPaid;
            payment.PaymentMethod = paymentMethod;
            payment.PaymentDate = DateTime.UtcNow;

            // --- FIX: Update the parent Booking entity status ---
            if (payment.Booking != null)
            {
                // Replace 'Confirmed' with whatever your Paid enum option is called in BookingStatus
                payment.Booking.Status = BookingStatus.Approved;
            }

            await _paymentRepository.SaveChangesAsync();
            return true;
        }
    }
}