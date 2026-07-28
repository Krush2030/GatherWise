using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.ViewModels;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "ServiceCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cart/Index
        public IActionResult Index(int bookingId)
        {
            var cart = GetCartFromSession();
            ViewBag.BookingId = bookingId;
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int serviceId, int bookingId)
        {
            var service = await _context.VendorServices
                .Include(s => s.Vendor)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null) return NotFound();

            var cart = GetCartFromSession();

            // Prevent adding duplicate items to the temporary cart
            if (!cart.Any(item => item.ServiceId == serviceId))
            {
                cart.Add(new CartItemViewModel
                {
                    ServiceId = service.Id,
                    ServiceName = service.ServiceName,
                    Category = service.ServiceCategory,
                    Price = service.BasePrice,
                    MainPhotoPath = service.MainPhotoPath,
                    VendorName = service.Vendor?.BusinessName ?? "Independent Vendor"
                });
                SaveCartToSession(cart);
            }

            return RedirectToAction("SelectServices", "Booking", new { bookingId = bookingId });
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int serviceId, int bookingId)
        {
            var cart = GetCartFromSession();
            var itemToRemove = cart.FirstOrDefault(i => i.ServiceId == serviceId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartToSession(cart);
            }

            return RedirectToAction(nameof(Index), new { bookingId = bookingId });
        }

        // POST: /Cart/ConfirmSelection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmSelection(int bookingId)
        {
            var cart = GetCartFromSession();
            if (!cart.Any()) return RedirectToAction("Index", new { bookingId = bookingId });

            foreach (var item in cart)
            {
                // Check if already saved in DB for this booking
                var exists = await _context.BookingServices
                    .AnyAsync(bs => bs.BookingId == bookingId && bs.VendorServiceId == item.ServiceId);

                if (!exists)
                {
                    var bookingServiceItem = new BookingService
                    {
                        BookingId = bookingId,
                        VendorServiceId = item.ServiceId,
                        PriceAtBooking = item.Price
                    };
                    _context.BookingServices.Add(bookingServiceItem);
                }
            }

            await _context.SaveChangesAsync();

            // Clear the cart session after success checkout
            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction("Index", "Booking"); // Returns to itinerary/dashboard overview
        }

        private List<CartItemViewModel> GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString(CartSessionKey);
            return sessionData == null ? new List<CartItemViewModel>() : JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData)!;
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}