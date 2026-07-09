using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;
using GatherWise.Domain.ViewModels;

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
                FullName = user.FullName, // Assuming FullName is a property on your ApplicationUser
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CurrentRole = primaryRole
            };

            // If Vendor, pull their business profile details
            if (primaryRole == "Vendor")
            {
                var vendorProfile = await _context.Vendors.FirstOrDefaultAsync(v => v.OwnerId == userId);
                if (vendorProfile != null)
                {
                    model.BusinessName = vendorProfile.BusinessName;
                    model.ContactName = vendorProfile.ContactName;
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

            // Strip out Identity validation for Email to prevent spoofing modifications
            ModelState.Remove("Email");
            ModelState.Remove("CurrentRole");

            if (!ModelState.IsValid) return View("Index", model);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // 1. Update core ApplicationUser fields
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

            // 2. Update role-specific databases if applicable
            if (User.IsInRole("Vendor"))
            {
                var vendorProfile = await _context.Vendors.FirstOrDefaultAsync(v => v.OwnerId == userId);
                if (vendorProfile != null)
                {
                    vendorProfile.BusinessName = model.BusinessName ?? string.Empty;
                    vendorProfile.ContactName = model.ContactName ?? model.FullName;
                    vendorProfile.Phone = model.PhoneNumber; // Keep synced

                    _context.Vendors.Update(vendorProfile);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Your profile settings have been successfully locked in!";
            return RedirectToAction(nameof(Index));
        }
    }
}