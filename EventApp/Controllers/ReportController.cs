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

        // GET: /Report/History (Accessible by Event Hosts / Venue Owners)
        public async Task<IActionResult> History()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Gather all reports submitted by the logged-in user
            var reports = await _context.Set<UserReport>()
                .Where(r => r.ReporterId == currentUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Build dynamic resolution maps for display
            var userMap = await _context.Users.ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName);
            ViewBag.UserMap = userMap;

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