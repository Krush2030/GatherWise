using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Services.Interfaces
{
    public interface IVendorAssignmentService
    {
        Task<VendorAssignment?> GetAssignmentByIdAsync(int id);
        Task<IEnumerable<VendorAssignment>> GetAssignmentsByBookingIdAsync(int bookingId);
        Task<IEnumerable<VendorAssignment>> GetAssignmentsByVendorIdAsync(int vendorId);
        Task<VendorAssignment> AssignVendorToBookingAsync(VendorAssignment assignment);
        Task UpdateAssignmentDetailsAsync(VendorAssignment assignment);
        Task RemoveVendorFromBookingAsync(int id);
    }
}