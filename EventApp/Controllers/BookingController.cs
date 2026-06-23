using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;
using GatherWise.Services.Interfaces;
using GatherWise.Domain.Entities;

namespace GatherWise.Web.Controllers
{
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
            var bookings = await _bookingService.GetAllBookingsAsync();
            return View(bookings);
        }

        // GET: /Booking/Create
        public async Task<IActionResult> Create()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            ViewBag.VenueId = new SelectList(venues, "Id", "Name");
            return View();
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,SlotId,EstimatedGuests,TotalPrice")] Booking booking)
        {
            // For now, since Identity isn't wired up yet, we'll assign a mock string Host ID
            booking.EventHostId = "MOCK_USER_123";

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

            // If validation fails, rebuild the venue dropdown collection
            var venues = await _venueService.GetAllVenuesAsync();
            ViewBag.VenueId = new SelectList(venues, "Id", "Name", booking.VenueId);
            return View(booking);
        }

        // AJAX Helper API Endpoint: /Booking/GetAvailableSlots?venueId=5
        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int venueId)
        {
            var allSlots = await _slotService.GetSlotsByVenueIdAsync(venueId);
            // Filter out already booked slots dynamically
            var availableSlots = new System.Collections.Generic.List<object>();

            foreach (var s in allSlots)
            {
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
    }
}