using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GatherWise.Services.Interfaces;
using GatherWise.Domain.Entities;

namespace GatherWise.Web.Controllers
{
    public class VenueController : Controller
    {
        private readonly IVenueService _venueService;

        public VenueController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        // GET: /Venue
        public async Task<IActionResult> Index()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            return View(venues);
        }

        // GET: /Venue/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // GET: /Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,Capacity,PricePerSlot,Description")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                await _venueService.CreateVenueAsync(venue);
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: /Venue/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: /Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Location,Capacity,PricePerSlot,Description,IsAvailable")] Venue venue)
        {
            if (id != venue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _venueService.UpdateVenueAsync(venue);
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: /Venue/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: /Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _venueService.DeleteVenueAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}