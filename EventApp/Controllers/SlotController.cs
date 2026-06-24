using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Security.Claims;
using GatherWise.Services.Interfaces;
using GatherWise.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class SlotController : Controller
    {
        private readonly ISlotService _slotService;
        private readonly IVenueService _venueService;

        public SlotController(ISlotService slotService, IVenueService venueService)
        {
            _slotService = slotService;
            _venueService = venueService;
        }

        // GET: /Slot
        public async Task<IActionResult> Index()
        {
            var slots = await _slotService.GetAllSlotsAsync();
            return View(slots);
        }

        // GET: /Slot/Create
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> Create()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var venues = await _venueService.GetAllVenuesAsync();

            // If they are an Admin, show all properties. If they are a Venue Owner, only show their own properties in the dropdown list!
            if (!User.IsInRole("Admin"))
            {
                venues = System.Linq.Enumerable.Where(venues, v => v.OwnerId == currentUserId);
            }

            ViewBag.VenueId = new SelectList(venues, "Id", "Name");
            return View();
        }

        // POST: /Slot/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> Create([Bind("VenueId,Date,StartTime,EndTime")] Slot slot)
        {
            var venue = await _venueService.GetVenueByIdAsync(slot.VenueId);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (venue == null || (!User.IsInRole("Admin") && venue.OwnerId != currentUserId))
            {
                return Forbid(); // Prevents unauthorized injection via raw post requests
            }

            if (ModelState.IsValid)
            {
                await _slotService.CreateSlotAsync(slot);
                return RedirectToAction(nameof(Index));
            }

            var venues = await _venueService.GetAllVenuesAsync();
            if (!User.IsInRole("Admin"))
            {
                venues = System.Linq.Enumerable.Where(venues, v => v.OwnerId == currentUserId);
            }
            ViewBag.VenueId = new SelectList(venues, "Id", "Name", slot.VenueId);
            return View(slot);
        }

        // GET: /Slot/Delete/5
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            var slot = await _slotService.GetSlotByIdAsync(id);
            if (slot == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && slot.Venue?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            return View(slot);
        }

        // POST: /Slot/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slot = await _slotService.GetSlotByIdAsync(id);
            if (slot == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && slot.Venue?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            await _slotService.DeleteSlotAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}