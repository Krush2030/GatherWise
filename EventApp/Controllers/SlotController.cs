using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using GatherWise.Services.Interfaces;
using GatherWise.Domain.Entities;

namespace GatherWise.Web.Controllers
{
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
        // Displays all created slots across venues
        public async Task<IActionResult> Index()
        {
            var slots = await _slotService.GetAllSlotsAsync();
            return View(slots);
        }

        // GET: /Slot/Create
        public async Task<IActionResult> Create()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            // Create a SelectList to populate the Venue dropdown in the form
            ViewBag.VenueId = new SelectList(venues, "Id", "Name");
            return View();
        }

        // POST: /Slot/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,Date,StartTime,EndTime")] Slot slot)
        {
            if (ModelState.IsValid)
            {
                await _slotService.CreateSlotAsync(slot);
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, reload the venue list dropdown
            var venues = await _venueService.GetAllVenuesAsync();
            ViewBag.VenueId = new SelectList(venues, "Id", "Name", slot.VenueId);
            return View(slot);
        }

        // GET: /Slot/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var slot = await _slotService.GetSlotByIdAsync(id);
            if (slot == null)
            {
                return NotFound();
            }
            return View(slot);
        }

        // POST: /Slot/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _slotService.DeleteSlotAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}