using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.ViewModels;
using GatherWise.Domain.Enums;

namespace GatherWise.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            string primaryRole = roles.Count > 0 ? roles[0] : "Event Host";

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CurrentRole = primaryRole
            };

            // ---- Role-Driven Dashboard Aggregation Pipeline ----
            if (primaryRole == "Event Host")
            {
                var hostBookingsQuery = _context.Set<Booking>()
                    .Where(b => b.EventHostId == userId);

                model.TotalBookingsCount = await hostBookingsQuery.CountAsync();

                model.TotalRevenueOrExpenditure = await hostBookingsQuery
                    .Where(b => b.Status == BookingStatus.Approved || b.Status.ToString() == "Paid")
                    .SumAsync(b => b.TotalPrice);
            }
            else if (primaryRole == "Venue Owner")
            {
                // Pull Top Venues mapped to this specific user context
                model.TopVenues = await _context.Venues
                    .Where(v => v.OwnerId == userId)
                    .Take(3)
                    .ToListAsync();

                model.TotalActiveListingsCount = await _context.Venues.CountAsync(v => v.OwnerId == userId);

                // Populate summary statistics from bookings matching owner venues
                model.TotalBookingsCount = await _context.Set<Booking>()
                    .CountAsync(b => b.Venue != null && b.Venue.OwnerId == userId);

                // FIXED: Filter out resolved reports so they disappear once the Admin clicks resolve
                model.AssociatedReports = await _context.UserReports
                    .Where(r => (r.ReporterId == userId || r.ReportedUserId == userId) && !r.IsResolved)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            else if (primaryRole == "Vendor")
            {
                var vendorProfile = await _context.Vendors
                    .Include(v => v.Services)
                    .FirstOrDefaultAsync(v => v.OwnerId == userId);

                if (vendorProfile != null)
                {
                    model.BusinessName = vendorProfile.BusinessName;
                    model.ContactName = vendorProfile.ContactName;

                    model.TopServices = vendorProfile.Services.Take(3).ToList();
                    model.TotalActiveListingsCount = vendorProfile.Services.Count;

                    model.TotalBookingsCount = await _context.Set<BookingService>()
                        .CountAsync(bs => bs.VendorService != null && bs.VendorService.VendorId == vendorProfile.Id);
                }
            }

            return View(model);
        }

        // POST: /Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (model.Id != userId) return Forbid();

            ModelState.Remove("Email");
            ModelState.Remove("CurrentRole");
            ModelState.Remove("TopVenues");
            ModelState.Remove("TopServices");

            if (!ModelState.IsValid) return View("Index", model);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Index", model);
            }

            if (User.IsInRole("Vendor"))
            {
                var vendorProfile = await _context.Vendors.FirstOrDefaultAsync(v => v.OwnerId == userId);
                if (vendorProfile != null)
                {
                    vendorProfile.BusinessName = model.BusinessName ?? string.Empty;
                    vendorProfile.ContactName = model.ContactName ?? model.FullName;
                    vendorProfile.Phone = model.PhoneNumber;

                    _context.Vendors.Update(vendorProfile);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Your profile settings have been successfully locked in!";
            return RedirectToAction(nameof(Index));
        }
    }
}