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
        private readonly IWebHostEnvironment _environment; // Injected for saving local items

        public VenueController(IVenueService venueService, IWebHostEnvironment environment)
        {
            _venueService = venueService;
            _environment = environment;
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
        //[Authorize(Roles = "Admin,Venue Owner")]
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: /Venue/Create
        //[Authorize(Roles = "Admin,Venue Owner")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Name,Location,Capacity,PricePerSlot,Description")] Venue venue)
        //{
        //    // Bind the logged-in owner's unique ID automatically
        //    venue.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        //    ModelState.Remove("OwnerId");

        //    if (ModelState.IsValid)
        //    {
        //        await _venueService.CreateVenueAsync(venue);
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(venue);
        //}

        [Authorize(Roles = "Admin,Venue Owner")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Venue Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,Capacity,PricePerSlot,Description")] Venue venue, IFormFileCollection files)
        {
            venue.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ModelState.Remove("OwnerId");

            // Enforce max 15 image count rule logic bounds
            if (files != null && files.Count > 15)
            {
                ModelState.AddModelError("Images", "You can upload a maximum of 15 images for a venue listing.");
            }

            if (ModelState.IsValid)
            {
                // 1. Save Base entity first to obtain operational scope ID configuration
                await _venueService.CreateVenueAsync(venue);

                // 2. Upload and save files loop 
                if (files != null && files.Count > 0)
                {
                    string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "venues");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                            string filePath = Path.Combine(uploadDir, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            venue.Images.Add(new VenueImage
                            {
                                ImagePath = "/uploads/venues/" + uniqueFileName,
                                VenueId = venue.Id
                            });
                        }
                    }
                    // Save relations updates changes
                    await _venueService.UpdateVenueAsync(venue);
                }

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