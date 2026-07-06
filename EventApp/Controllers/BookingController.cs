using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GatherWise.Domain.Entities;
using GatherWise.Services.Interfaces;
using GatherWise.DataAccess.Data; // Ensure this matches your context path

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IVenueService _venueService;
        private readonly ISlotService _slotService;
        private readonly ApplicationDbContext _context; // Injected to resolve the context error

        public BookingController(
            IBookingService bookingService,
            IVenueService venueService,
            ISlotService slotService,
            ApplicationDbContext context) // Add context here
        {
            _bookingService = bookingService;
            _venueService = venueService;
            _slotService = slotService;
            _context = context;
        }

        // GET: /Booking
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (User.IsInRole("Event Host"))
            {
                var hostBookings = await _bookingService.GetBookingsByHostIdAsync(currentUserId);
                return View(hostBookings);
            }

            if (User.IsInRole("Venue Owner"))
            {
                var ownerBookings = await _bookingService.GetBookingsByOwnerIdAsync(currentUserId);
                return View(ownerBookings);
            }

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
            booking.EventHostId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            ModelState.Remove("EventHostId");
            ModelState.Remove("Status");
            ModelState.Remove("CreatedAt");

            var targetSlot = await _slotService.GetSlotByIdAsync(booking.SlotId);
            if (targetSlot == null || targetSlot.IsBooked)
            {
                ModelState.AddModelError("SlotId", "This operational slot has already been locked or confirmed by another user.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Create the base venue booking
                    await _bookingService.CreateBookingAsync(booking);

                    // 2. Redirect to Vendor Services Selection right away, passing the new Booking Id!
                    return RedirectToAction(nameof(SelectServices), new { bookingId = booking.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var venues = await _venueService.GetAllVenuesAsync();
            ViewBag.VenueId = new SelectList(venues, "Id", "Name", booking.VenueId);
            return View(booking);
        }

        // GET: /Booking/GetAvailableSlots
        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int venueId)
        {
            var allSlots = await _slotService.GetSlotsByVenueIdAsync(venueId);
            var slotData = new System.Collections.Generic.List<object>();

            foreach (var s in allSlots)
            {
                slotData.Add(new
                {
                    id = s.Id,
                    date = s.Date.ToString("dd-MMM-yyyy"),
                    startTime = s.StartTime.ToString(@"hh\:mm"),
                    endTime = s.EndTime.ToString(@"hh\:mm"),
                    isBooked = s.IsBooked
                });
            }
            return Json(slotData);
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

        // GET: /Booking/SelectServices?bookingId=5
        [Authorize(Roles = "Admin,Event Host")]
        public async Task<IActionResult> SelectServices(int bookingId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);

            if (booking == null || booking.EventHostId != currentUserId)
            {
                return NotFound();
            }

            // Successfully fetches all active vendor services via the newly injected context
            var services = await _context.VendorServices.Include(s => s.Vendor).ToListAsync();

            ViewBag.BookingId = bookingId;
            return View(services);
        }

        // POST: /Booking/BookService
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Event Host")]
        public async Task<IActionResult> BookService(int bookingId, int serviceId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.EventHostId == currentUserId);
            var service = await _context.VendorServices.FindAsync(serviceId);

            if (booking == null || service == null)
            {
                return NotFound();
            }

            var alreadyBooked = await _context.BookingServices.AnyAsync(bs => bs.BookingId == bookingId && bs.VendorServiceId == serviceId);
            if (!alreadyBooked)
            {
                var bookingServiceItem = new BookingService
                {
                    BookingId = bookingId,
                    VendorServiceId = serviceId,
                    PriceAtBooking = service.BasePrice
                };

                _context.BookingServices.Add(bookingServiceItem);

                // Appends the vendor service base cost to the global reservation checkout total balance
                booking.TotalPrice += service.BasePrice;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(SelectServices), new { bookingId = bookingId });
        }

        // GET: /Booking/SummaryDetails/5
        public async Task<IActionResult> SummaryDetails(int id)
        {
            var detailedBooking = await _bookingService.GetWithDetailsByIdAsync(id);

            if (detailedBooking == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && detailedBooking.EventHostId != currentUserId && detailedBooking.Venue?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            return View(detailedBooking);
        }
    }
}