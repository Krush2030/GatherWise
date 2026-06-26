using System.Threading.Tasks;
using GatherWise.Domain.ViewModels;

namespace GatherWise.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetHostDashboardDataAsync(string hostId);
        Task<DashboardViewModel> GetOwnerDashboardDataAsync(string ownerId);
    }
}