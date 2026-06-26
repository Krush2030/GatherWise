using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using GatherWise.Services.Interfaces;
using GatherWise.Domain.ViewModels;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            DashboardViewModel model;

            if (User.IsInRole("Venue Owner"))
            {
                model = await _dashboardService.GetOwnerDashboardDataAsync(userId);
            }
            else // Default or Event Host view mapping fallbacks
            {
                model = await _dashboardService.GetHostDashboardDataAsync(userId);
            }

            return View(model);
        }
    }
}