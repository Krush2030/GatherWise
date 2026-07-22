using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GatherWise.Services.Interfaces;
using GatherWise.Domain.Enums;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService; // Injected for updating booking status on timeout
        private readonly ISlotService _slotService;       // Injected for releasing slots back to pool

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService,
            ISlotService slotService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _slotService = slotService;
        }

        // GET: /Payment
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (User.IsInRole("Event Host"))
            {
                var hostInvoices = await _paymentService.GetPaymentsByHostIdAsync(currentUserId);
                return View(hostInvoices);
            }

            if (User.IsInRole("Venue Owner"))
            {
                var ownerInvoices = await _paymentService.GetPaymentsByOwnerIdAsync(currentUserId);
                return View(ownerInvoices);
            }

            return Forbid();
        }

        // GET: /Payment/Checkout/5
        [Authorize(Roles = "Event Host")]
        public async Task<IActionResult> Checkout(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // Ensure the checkout form belongs to the logged-in host
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (payment.Booking?.EventHostId != currentUserId)
            {
                return Forbid();
            }

            if (payment.Status == PaymentStatus.FullyPaid)
            {
                TempData["InfoMessage"] = "This invoice has already been fully cleared.";
                return RedirectToAction(nameof(Index));
            }

            return View(payment);
        }

        // POST: /Payment/Checkout/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Event Host")]
        public async Task<IActionResult> Checkout(int id, string paymentMethod)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (payment.Booking?.EventHostId != currentUserId)
            {
                return Forbid();
            }

            var booking = payment.Booking;
            if (booking == null)
            {
                return NotFound("Associated booking details could not be found.");
            }

            // --- 1-HOUR PAYMENT CUT-OFF WORKFLOW ---
            if (booking.Status != BookingStatus.Approved || !booking.ApprovedAt.HasValue)
            {
                ModelState.AddModelError("", "This reservation is not in an approved state ready for payment.");
                return View(payment);
            }

            // Evaluate if event host has missed the 60-minute window (using UTC to align with db system)
            var timeElapsedSinceApproval = DateTime.UtcNow - booking.ApprovedAt.Value;

            if (timeElapsedSinceApproval.TotalHours > 1)
            {
                // Timeout exceeded. Release the structural slot back into the public pool!
                booking.Status = BookingStatus.CancelledByTimeout;

                if (booking.Slot != null)
                {
                    booking.Slot.IsBooked = false; // Make slot available again
                    await _slotService.UpdateSlotAsync(booking.Slot);
                }

                await _bookingService.UpdateBookingStatusAsync(booking.Id, BookingStatus.CancelledByTimeout);

                TempData["ErrorMessage"] = "The 1-hour payment window for this reservation has expired. The reservation has been canceled and the slot has been released.";
                return RedirectToAction("Index", "Venue");
            }
            // ----------------------------------------

            // Proceed with payment execution logic safely...
            var success = await _paymentService.ProcessPaymentAsync(id, paymentMethod);
            if (success)
            {
                TempData["SuccessMessage"] = "Payment processed successfully! Your booking is locked.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to process payment request.");
            return View(payment);
        }

        // GET: /Payment/Receipt/5
        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Security Guard: Only allow the Admin, the Event Host who paid, or the Venue Owner who gets the money to view this receipt
            if (!User.IsInRole("Admin") &&
                payment.Booking?.EventHostId != currentUserId &&
                payment.Booking?.Venue?.OwnerId != currentUserId)
            {
                return Forbid();
            }

            return View(payment);
        }
    }
}