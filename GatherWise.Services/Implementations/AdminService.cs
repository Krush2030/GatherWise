using System.Threading.Tasks;
using GatherWise.Domain.Interfaces;
using GatherWise.Domain.ViewModels;
using GatherWise.Services.Interfaces;

namespace GatherWise.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<AdminUserDashboardViewModel> GetDashboardDataAsync()
        {
            return await _adminRepository.GetCategorizedUsersAsync();
        }

        public async Task<UserDisplayInfo> GetUserDetailsAsync(string userId)
        {
            return await _adminRepository.GetUserByIdAsync(userId);
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            return await _adminRepository.ToggleUserStatusAsync(userId);
        }
    }
}