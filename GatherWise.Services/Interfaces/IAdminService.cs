using System.Threading.Tasks;
using GatherWise.Domain.ViewModels;

namespace GatherWise.Services.Interfaces
{
    public interface IAdminService
    {
        /// <summary>
        /// Orchestrates the retrieval and business preparation of categorized application users for the administrator dashboard.
        /// </summary>
        Task<AdminUserDashboardViewModel> GetDashboardDataAsync();

        /// <summary>
        /// Obtains complete tracking metrics for a specific system client identity.
        /// </summary>
        Task<UserDisplayInfo> GetUserDetailsAsync(string userId);

        /// <summary>
        /// Updates the blacklisting flag status constraint on an account entity record.
        /// </summary>
        Task<bool> ToggleUserStatusAsync(string userId);
    }
}