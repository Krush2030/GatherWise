using System.Collections.Generic;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;

namespace GatherWise.Domain.Interfaces
{
    public interface IVendorRepository
    {
        Task<Vendor?> GetVendorByOwnerIdAsync(string ownerId);
        Task CreateVendorProfileAsync(Vendor vendor);
        Task AddServiceAsync(VendorService service);
        Task<IEnumerable<VendorService>> GetServicesByVendorIdAsync(int vendorId);
    }
}