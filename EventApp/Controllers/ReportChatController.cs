using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Interfaces;

namespace GatherWise.Web.Controllers
{
    [Authorize(Roles = "Admin,Venue Owner")]
    public class ReportChatController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly ApplicationDbContext _context;

        public ReportChatController(IChatRepository chatRepository, ApplicationDbContext context)
        {
            _chatRepository = chatRepository;
            _context = context;
        }

        // GET: /ReportChat/TicketRoom/5
        public async Task<IActionResult> TicketRoom(int id)
        {
            var report = await _context.Set<UserReport>().FindAsync(id);
            if (report == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validation Guard: If the user is a Venue Owner, ensure they own the venue associated with the issue or are the reported user/reporter
            if (User.IsInRole("Venue Owner"))
            {
                // Verify if this report concerns them directly
                if (report.ReporterId != currentUserId && report.ReportedUserId != currentUserId)
                {
                    return Forbid();
                }
            }

            var messages = await _chatRepository.GetChatHistoryByReportIdAsync(id);

            // Build simple lookup dictionary for users to show real names in the UI chat boxes
            ViewBag.SystemUsers = await _context.Users.ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName);
            ViewBag.CurrentReport = report;

            return View(messages);
        }

        // POST: /ReportChat/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int reportId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return BadRequest("Message content cannot be empty.");
            }

            var report = await _context.Set<UserReport>().FindAsync(reportId);
            if (report == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validation Guard Duplicate for Post Channel
            if (User.IsInRole("Venue Owner") && report.ReporterId != currentUserId && report.ReportedUserId != currentUserId)
            {
                return Forbid();
            }

            var message = new AdminOwnerChatMessage
            {
                UserReportId = reportId,
                SenderId = currentUserId ?? string.Empty,
                MessageText = messageText.Trim(),
                SentAt = DateTime.UtcNow
            };

            await _chatRepository.AddMessageAsync(message);

            return RedirectToAction(nameof(TicketRoom), new { id = reportId });
        }
    }
}