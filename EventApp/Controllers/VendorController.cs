using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Domain.ViewModels;
using GatherWise.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using GatherWise.DataAccess.Data;

namespace GatherWise.Web.Controllers
{
    [Authorize(Roles = "Admin,Vendor")]
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public VendorController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Vendor/MyServices
        public async Task<IActionResult> MyServices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vendorProfile = await _context.Vendors
                .Include(v => v.Services)
                .FirstOrDefaultAsync(v => v.OwnerId == userId);

            if (vendorProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            return View(vendorProfile.Services);
        }

        // GET: /Vendor/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch the specific service with its gallery images and parent vendor profile
            var serviceDetails = await _context.VendorServices
                .Include(s => s.GalleryImages)
                .Include(s => s.Vendor)
                .FirstOrDefaultAsync(s => s.Id == id && s.Vendor.OwnerId == userId);

            if (serviceDetails == null)
            {
                return NotFound();
            }

            return View(serviceDetails);
        }

        // GET: /Vendor/CreateProfile
        public IActionResult CreateProfile() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile([Bind("BusinessName,ContactName,Email,Phone")] Vendor vendor)
        {
            vendor.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                _context.Vendors.Add(vendor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyServices));
            }
            return View(vendor);
        }

        // GET: /Vendor/AddService
        public async Task<IActionResult> AddService()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vendorExists = await _context.Vendors.AnyAsync(v => v.OwnerId == userId);
            if (!vendorExists) return RedirectToAction(nameof(CreateProfile));

            return View();
        }

        // POST: /Vendor/AddService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(VendorServiceViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.OwnerId == userId);

            if (vendor == null) return RedirectToAction(nameof(CreateProfile));

            if (ModelState.IsValid)
            {
                string mainPhotoFileName = await UploadFileAsync(model.MainPhoto, "main_services");

                var serviceEntity = new VendorService
                {
                    VendorId = vendor.Id,
                    ServiceName = model.ServiceName,
                    ServiceCategory = model.ServiceCategory,
                    ServicePhone = model.ServicePhone,
                    BasePrice = model.BasePrice,
                    PerNumberOfPersons = model.PerNumberOfPersons,
                    MainPhotoPath = mainPhotoFileName
                };

                if (model.GalleryImages != null && model.GalleryImages.Count > 0)
                {
                    foreach (var file in model.GalleryImages)
                    {
                        string galleryPath = await UploadFileAsync(file, "gallery_services");
                        serviceEntity.GalleryImages.Add(new VendorServiceImage { ImagePath = galleryPath });
                    }
                }

                _context.VendorServices.Add(serviceEntity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyServices));
            }

            return View(model);
        }

        private async Task<string> UploadFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
    }
}