using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Report/Create?reportedUserId=xyz
        public async Task<IActionResult> Create(string reportedUserId)
        {
            if (string.IsNullOrEmpty(reportedUserId)) return BadRequest();

            var targetUser = await _userManager.FindByIdAsync(reportedUserId);
            if (targetUser == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == reportedUserId)
            {
                TempData["Error"] = "You cannot report yourself.";
                return RedirectToAction("Index", "Profile");
            }

            ViewBag.TargetUserId = targetUser.Id;
            ViewBag.TargetUserName = targetUser.FullName ?? targetUser.UserName;
            ViewBag.TargetPhone = targetUser.PhoneNumber ?? "N/A";

            return View();
        }

        // POST: /Report/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string reportedUserId, string description)
        {
            if (string.IsNullOrEmpty(reportedUserId) || string.IsNullOrEmpty(description))
            {
                ModelState.AddModelError("", "Description and Target parameters are required.");
                return View();
            }

            var report = new UserReport
            {
                ReporterId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ReportedUserId = reportedUserId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<UserReport>().Add(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your report has been successfully transmitted to systemic administration.";
            return RedirectToAction("History");
        }

        // GET: /Report/History
        public async Task<IActionResult> History()
        {
            // 1. Get the current logged-in user's ID
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Challenge();

            // 2. Fetch all reports related to this specific user context (both sent and received)
            var reports = await _context.Set<UserReport>()
                .Where(r => r.ReporterId == currentUserId || r.ReportedUserId == currentUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // 3. Extract unique IDs to construct the UserMap mapping dictionary for names
            var involvedUserIds = reports.Select(r => r.ReportedUserId)
                .Concat(reports.Select(r => r.ReporterId))
                .Distinct()
                .ToList();

            var userMap = await _context.Users
                .Where(u => involvedUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? "Unknown Account");

            // 4. Send mappings to view through ViewBag
            ViewBag.UserMap = userMap;
            ViewBag.CurrentUserId = currentUserId;

            return View(reports);
        }

        // POST: /Report/Resolve (Admin Console Action Only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id, string adminReply)
        {
            var report = await _context.Set<UserReport>().FindAsync(id);
            if (report == null) return NotFound();

            report.IsResolved = true;
            report.AdminReply = adminReply ?? "Resolved by Admin.";
            report.ResolvedAt = DateTime.UtcNow;

            _context.Set<UserReport>().Update(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Report status updated and feedback routed successfully.";
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}