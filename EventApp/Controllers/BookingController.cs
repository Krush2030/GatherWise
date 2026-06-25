using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using GatherWise.Services.Interfaces;
using GatherWise.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IVenueService _venueService;
        private readonly ISlotService _slotService;

        public BookingController(IBookingService bookingService, IVenueService venueService, ISlotService slotService)
        {
            _bookingService = bookingService;
            _venueService = venueService;
            _slotService = slotService;
        }

        // GET: /Booking
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // 1. If Event Host: They see only the reservations they personally created
            if (User.IsInRole("Event Host"))
            {
                var hostBookings = await _bookingService.GetBookingsByHostIdAsync(currentUserId);
                return View(hostBookings);
            }

            // 2. If Venue Owner: They see only incoming requests targeted at their owned properties
            if (User.IsInRole("Venue Owner"))
            {
                // Ensure your IBookingService contains this filter or use your context to load records:
                var ownerBookings = await _bookingService.GetBookingsByOwnerIdAsync(currentUserId);
                return View(ownerBookings);
            }

            // 3. Admins or Platform Coordinators continue to see all systemic rows
            var allBookings = await _bookingService.GetAllBookingsAsync();
            return View(allBookings);
        }

        // GET: /Booking/Create
        [Authorize(Roles = "Admin,Event Host")]
        public async Task<IActionResult> Create()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            ViewBag.VenueId = new SelectList(venues, "Id", "Name");
            return View();
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Event Host")]
        public async Task<IActionResult> Create([Bind("VenueId,SlotId,EstimatedGuests,TotalPrice")] Booking booking)
        {
            // Dynamically grab the real database ID of the currently logged-in user
            booking.EventHostId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // Remove internal tracking fields from ModelState validation rules
            ModelState.Remove("EventHostId");
            ModelState.Remove("Status");
            ModelState.Remove("CreatedAt");

            // SERVER-SIDE GUARD: Double check the database status directly before allowing creation logic
            var targetSlot = await _slotService.GetSlotByIdAsync(booking.SlotId);
            if (targetSlot == null || targetSlot.IsBooked)
            {
                ModelState.AddModelError("SlotId", "This operational slot has already been locked or confirmed by another user.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookingService.CreateBookingAsync(booking);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // If validation fails, rebuild the venue dropdown collection safely
            var venues = await _venueService.GetAllVenuesAsync();
            ViewBag.VenueId = new SelectList(venues, "Id", "Name", booking.VenueId);
            return View(booking);
        }

        // GET: /Booking/GetAvailableSlots
        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int venueId)
        {
            // Fetch slots directly from the DB context bypassing local cached tracking arrays
            var allSlots = await _slotService.GetSlotsByVenueIdAsync(venueId);
            var availableSlots = new System.Collections.Generic.List<object>();

            foreach (var s in allSlots)
            {
                // Now that Step 1 forces IsBooked to save as true, this condition will correctly filter it out!
                if (!s.IsBooked)
                {
                    availableSlots.Add(new
                    {
                        id = s.Id,
                        text = $"{s.Date.ToString("dd-MMM-yyyy")} ({s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm})"
                    });
                }
            }
            return Json(availableSlots);
        }

        // POST: /Booking/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> Approve(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && booking.Venue?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            await _bookingService.UpdateBookingStatusAsync(id, GatherWise.Domain.Enums.BookingStatus.Confirmed);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Booking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Venue Owner,Event Host")]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && booking.EventHostId != currentUserId && booking.Venue?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            await _bookingService.CancelBookingAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}