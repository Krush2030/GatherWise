using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GatherWise.Domain.Entities;
using GatherWise.Domain.Enums;
using GatherWise.Domain.ViewModels; 
using GatherWise.Services.Interfaces;
using GatherWise.DataAccess.Data;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IVenueService _venueService;
        private readonly ISlotService _slotService;
        private readonly ApplicationDbContext _context;

        public BookingController(
            IBookingService bookingService,
            IVenueService venueService,
            ISlotService slotService,
            ApplicationDbContext context)
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
            IEnumerable<Booking> bookings;

            if (User.IsInRole("Event Host"))
            {
                bookings = await _bookingService.GetBookingsByHostIdAsync(currentUserId);
            }
            else if (User.IsInRole("Venue Owner"))
            {
                bookings = await _bookingService.GetBookingsByOwnerIdAsync(currentUserId);
            }
            else
            {
                bookings = await _bookingService.GetAllBookingsAsync();
            }

            // Fetch payment records for these bookings to pass their IDs to the view
            var bookingIds = bookings.Select(b => b.Id).ToList();

            // Grabs payment mappings regardless of status to prevent "Invoice Error" text rendering
            var paymentMappings = await _context.Payments
                .Where(p => bookingIds.Contains(p.BookingId))
                .ToDictionaryAsync(p => p.BookingId, p => p.Id);

            ViewBag.PaymentIds = paymentMappings;

            return View(bookings);
        }

        // GET: /Booking/Create?venueId=5&slotId=12
        [Authorize(Roles = "Admin,Event Host")]
        public async Task<IActionResult> Create(int? venueId, int? slotId)
        {
            var venues = await _venueService.GetAllVenuesAsync();

            var booking = new Booking();
            if (venueId.HasValue)
            {
                booking.VenueId = venueId.Value;
                var targetedVenue = await _venueService.GetVenueByIdAsync(venueId.Value);
                if (targetedVenue != null)
                {
                    booking.TotalPrice = targetedVenue.PricePerSlot;
                    booking.EstimatedGuests = targetedVenue.Capacity; // Prefill model capacity if coming directly from Venue Details
                }
            }
            if (slotId.HasValue)
            {
                booking.SlotId = slotId.Value;
            }

            ViewBag.VenueId = new SelectList(venues, "Id", "Name", booking.VenueId);

            var slots = venueId.HasValue ? await _slotService.GetSlotsByVenueIdAsync(venueId.Value) : new List<Slot>();
            ViewBag.SlotsList = slots;

            return View(booking);
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
                    await _bookingService.CreateBookingAsync(booking);
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
            var targetedVenue = await _venueService.GetVenueByIdAsync(venueId);
            var currentDateTime = DateTime.UtcNow;

            // Extract the venue's configured guest capacity directly from the object
            int venueCapacityValue = targetedVenue?.Capacity ?? 0;

            var activeAvailableSlots = allSlots.Where(s =>
                !s.IsBooked &&
                (s.Date.Date > currentDateTime.Date ||
                (s.Date.Date == currentDateTime.Date && s.StartTime > currentDateTime.TimeOfDay))
            );

            var slotData = new List<object>();
            foreach (var s in activeAvailableSlots)
            {
                slotData.Add(new
                {
                    id = s.Id,
                    date = s.Date.ToString("dd-MMM-yyyy"),
                    startTime = s.StartTime.ToString(@"hh\:mm"),
                    endTime = s.EndTime.ToString(@"hh\:mm"),
                    isBooked = s.IsBooked,
                    venueCapacity = venueCapacityValue // Injected here to resolve UI reflection issues
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

            await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Approved);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Booking/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Venue Owner")]
        public async Task<IActionResult> Reject(int id)
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

            await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Rejected);
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

                // 1. Update the booking entity's cumulative tracker
                booking.TotalPrice += service.BasePrice;

                // --- FIX: Find and update the associated pending payment record ---
                var pendingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.BookingId == bookingId && p.Status == PaymentStatus.Pending);

                if (pendingPayment != null)
                {
                    pendingPayment.Amount = booking.TotalPrice;
                }
                // -----------------------------------------------------------------

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(SelectServices), new { bookingId = bookingId });
        }

        // GET: /Booking/SummaryDetails/5
        public async Task<IActionResult> SummaryDetails(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var detailedBooking = await _bookingService.GetWithDetailsByIdAsync(id);

            if (detailedBooking == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && detailedBooking.EventHostId != currentUserId && detailedBooking.Venue?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            // 🚀 NEW FLUSH LOGIC: Transfer Amazon-Session Cart items to Database
            var sessionData = HttpContext.Session.GetString("ServiceCart");
            if (!string.IsNullOrEmpty(sessionData))
            {
                var cartItems = JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData);

                if (cartItems != null && cartItems.Any())
                {
                    bool databaseUpdated = false;

                    foreach (var item in cartItems)
                    {
                        // Check if this service is already linked to the booking to prevent double entries
                        var alreadyBooked = await _context.BookingServices
                            .AnyAsync(bs => bs.BookingId == id && bs.VendorServiceId == item.ServiceId);

                        if (!alreadyBooked)
                        {
                            var vendorService = await _context.VendorServices.FindAsync(item.ServiceId);
                            if (vendorService != null)
                            {
                                // 1. Add record to relation matrix mapping table
                                var bookingServiceItem = new BookingService
                                {
                                    BookingId = id,
                                    VendorServiceId = item.ServiceId,
                                    PriceAtBooking = vendorService.BasePrice
                                };
                                _context.BookingServices.Add(bookingServiceItem);

                                // 2. Increment cumulative totals directly inside the tracked booking model
                                detailedBooking.TotalPrice += vendorService.BasePrice;
                                databaseUpdated = true;
                            }
                        }
                    }

                    if (databaseUpdated)
                    {
                        // 3. Keep the Invoice Billing system synced
                        var pendingPayment = await _context.Payments
                            .FirstOrDefaultAsync(p => p.BookingId == id && p.Status == PaymentStatus.Pending);

                        if (pendingPayment != null)
                        {
                            pendingPayment.Amount = detailedBooking.TotalPrice;
                        }

                        // 4. Save tracking alterations to DB
                        await _context.SaveChangesAsync();
                    }

                    // 5. Clear out session cart so subsequent page views don't repeat the loop calculation
                    HttpContext.Session.Remove("ServiceCart");
                }
            }

            // --- 1-Hour Pending Workflow Logic ---
            var currentDateTime = DateTime.UtcNow;
            var hoursSinceBooking = (currentDateTime - detailedBooking.CreatedAt).TotalHours;

            if (detailedBooking.Status == BookingStatus.PendingApproval && hoursSinceBooking >= 1)
            {
                ViewBag.ShowOwnerContact = true;
                ViewBag.OwnerPhoneNumber = detailedBooking.Venue?.Owner?.PhoneNumber ?? "Contact details unavailable";
            }
            else
            {
                ViewBag.ShowOwnerContact = false;
            }

            return View(detailedBooking);
        }
    }
}