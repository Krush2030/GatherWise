using System.Threading.Tasks;
using GatherWise.Domain.ViewModels;

namespace GatherWise.Domain.Interfaces
{
    public interface IAdminRepository
    {
        /// <summary>
        /// Retrieves all registered Venue Owners, Event Hosts, and Vendors categorized by their Identity Roles.
        /// </summary>
        Task<AdminUserDashboardViewModel> GetCategorizedUsersAsync();

        /// <summary>
        /// Looks up full system operational details for a targeted application user.
        /// </summary>
        Task<UserDisplayInfo> GetUserByIdAsync(string userId);

        /// <summary>
        /// Toggles the suspension / blacklisting state of a target user account.
        /// </summary>
        Task<bool> ToggleUserStatusAsync(string userId);
    }
}