using System.Linq; // Added for basic Where and OrderBy filters
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Added for native EF Core Async Extensions

namespace GatherWise.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = await _adminService.GetDashboardDataAsync();

            // Asynchronously pull database unresolved user report entries directly
            using (var scopeContext = new GatherWise.DataAccess.Data.ApplicationDbContext(
                HttpContext.RequestServices.GetRequiredService<DbContextOptions<GatherWise.DataAccess.Data.ApplicationDbContext>>()))
            {
                // Explicitly targeting Microsoft.EntityFrameworkCore extensions clears out standard Linq.Async confusion
                var unresolvedReports = await EntityFrameworkQueryableExtensions.ToListAsync(
                    scopeContext.Set<UserReport>()
                        .Where(r => !r.IsResolved)
                        .OrderByDescending(r => r.CreatedAt)
                );

                var systemUsers = await EntityFrameworkQueryableExtensions.ToDictionaryAsync(
                    scopeContext.Users,
                    u => u.Id,
                    u => u.FullName ?? u.UserName
                );

                ViewBag.UnresolvedReports = unresolvedReports;
                ViewBag.SystemUsers = systemUsers;
            }

            return View(model);
        }

        // GET: /Admin/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _adminService.GetUserDetailsAsync(id);
            if (user == null) return NotFound();

            return PartialView("_UserDetailsPartial", user);
        }

        // POST: /Admin/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var success = await _adminService.ToggleUserStatusAsync(id);
            if (!success) TempData["Error"] = "Failed to update user profile operational parameters.";
            else TempData["Success"] = "Account status metrics modified successfully.";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}