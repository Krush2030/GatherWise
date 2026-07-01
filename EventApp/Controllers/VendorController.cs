using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GatherWise.DataAccess.Data;
using GatherWise.Domain.Entities;

namespace GatherWise.Web.Controllers
{
    [Authorize(Roles = "Admin,Vendor")]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Vendor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Vendor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusinessName,ServiceCategory,ContactName,Email,Phone,BasePrice")] Vendor vendor)
        {
            // Bind the logged-in Vendor's unique Identity User ID automatically
            vendor.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                _context.Vendors.Add(vendor);
                await _context.SaveChangesAsync();

                // Redirects them back to home or their dashboard
                return RedirectToAction("Index", "Home");
            }
            return View(vendor);
        }
    }
}