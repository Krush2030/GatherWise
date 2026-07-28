using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Interfaces;
using GatherWise.Domain.ViewModels;

namespace GatherWise.DataAccess.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminUserDashboardViewModel> GetCategorizedUsersAsync()
        {
            var model = new AdminUserDashboardViewModel();

            // 1. Fetch all users along with their designated Role Name and Lockout Status
            var usersWithRoles = await (from user in _context.Users
                                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                        join role in _context.Roles on userRole.RoleId equals role.Id
                                        select new
                                        {
                                            user.Id,
                                            FullName = user.UserName, // Map to your custom user property if applicable
                                            user.Email,
                                            user.PhoneNumber,
                                            user.LockoutEnd,
                                            RoleName = role.Name
                                        }).ToListAsync();

            // 2. Map and categorize users into our ViewModel arrays
            foreach (var item in usersWithRoles)
            {
                // A user is operational/active if they aren't currently locked out
                bool isActive = item.LockoutEnd == null || item.LockoutEnd <= DateTimeOffset.UtcNow;

                var userInfo = new UserDisplayInfo
                {
                    Id = item.Id,
                    FullName = item.FullName ?? "N/A",
                    Email = item.Email ?? "N/A",
                    PhoneNumber = item.PhoneNumber ?? "N/A",
                    IsActive = isActive
                };

                switch (item.RoleName)
                {
                    case "Venue Owner":
                        model.VenueOwners.Add(userInfo);
                        break;
                    case "Event Host":
                        model.EventHosts.Add(userInfo);
                        break;
                    case "Vendor":
                        model.Vendors.Add(userInfo);
                        break;
                }
            }

            return model;
        }

        public async Task<UserDisplayInfo> GetUserByIdAsync(string userId)
        {
            var userDetail = await (from user in _context.Users
                                    join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                    join role in _context.Roles on userRole.RoleId equals role.Id
                                    where user.Id == userId
                                    select new UserDisplayInfo
                                    {
                                        Id = user.Id,
                                        FullName = user.UserName,
                                        Email = user.Email ?? "N/A",
                                        PhoneNumber = user.PhoneNumber ?? "N/A",
                                        IsActive = (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
                                    }).FirstOrDefaultAsync();

            return userDetail;
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Banning toggles between unlocking immediately and forcing a far-future lockout date
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                user.LockoutEnd = null; // Unblacklisted / Restored
            }
            else
            {
                user.LockoutEnd = new DateTimeOffset(new DateTime(2099, 12, 31)); // Blacklisted / Suspended
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}