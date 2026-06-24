using System.Threading.Tasks;
using System.Security.Claims;
using GatherWise.Domain.Entities;
using GatherWise.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatherWise.Web.Controllers
{
    [Authorize]
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
        [Authorize(Roles = "Admin,Venue Owner")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Venue/Create
        [Authorize(Roles = "Admin,Venue Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,Capacity,PricePerSlot,Description")] Venue venue)
        {
            // Bind the logged-in owner's unique ID automatically
            venue.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                await _venueService.CreateVenueAsync(venue);
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: /Venue/Edit/5
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> Edit(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            // Check Ownership
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && venue.OwnerId != currentUserId)
            {
                return Forbid(); // Blocks other Venue Owners completely
            }

            return View(venue);
        }

        // POST: /Venue/Edit/5
        [Authorize(Roles = "Admin,Venue Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Location,Capacity,PricePerSlot,Description,IsAvailable,OwnerId")] Venue venue)
        {
            if (id != venue.Id)
            {
                return NotFound();
            }

            // Check Ownership
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && venue.OwnerId != currentUserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                await _venueService.UpdateVenueAsync(venue);
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: /Venue/Delete/5
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            // Check Ownership
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && venue.OwnerId != currentUserId)
            {
                return Forbid();
            }

            return View(venue);
        }

        // POST: /Venue/Delete/5
        [Authorize(Roles = "Admin,Venue Owner")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            // Check Ownership
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && venue.OwnerId != currentUserId)
            {
                return Forbid();
            }

            await _venueService.DeleteVenueAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}