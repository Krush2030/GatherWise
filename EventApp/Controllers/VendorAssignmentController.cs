using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using GatherWise.Domain.Entities;
using GatherWise.Services.Interfaces;
using GatherWise.Web.Models;

[Authorize(Roles = "Admin")]
public class VendorAssignmentController : Controller
{
    private readonly IVendorAssignmentService _assignmentService;
    private readonly IBookingService _bookingService; // Assuming this exists to fetch bookings
    // Inject your IVendorService here if you have one, or use DbContext directly for metadata lookup

    public VendorAssignmentController(IVendorAssignmentService assignmentService, IBookingService bookingService)
    {
        _assignmentService = assignmentService;
        _bookingService = bookingService;
    }

    // GET: VendorAssignment/Create
    public async Task<IActionResult> Create()
    {
        // Mocking or fetching data for the dropdown lists
        var bookings = await _bookingService.GetAllBookingsAsync();
        // Note: Map or pass lists of bookings and vendors grouped by their types (Caterers, Decorators, etc.)

        var viewModel = new AssignVendorViewModel
        {
            // For select options display: "Booking #3 - Luxury Hall (Client Name)"
            BookingsList = new SelectList(bookings, "Id", "Id"),
            // VendorsList should display vendor Name + Category string
            VendorsList = new SelectList(new List<Vendor>(), "Id", "Name")
        };

        return View(viewModel);
    }

    // POST: VendorAssignment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignVendorViewModel model)
    {
        if (ModelState.IsValid)
        {
            var assignment = new VendorAssignment
            {
                BookingId = model.BookingId,
                VendorId = model.VendorId,
                FinalAgreedPrice = model.FinalAgreedPrice,
                SpecialInstructions = model.SpecialInstructions
            };

            await _assignmentService.AssignVendorToBookingAsync(assignment);
            return RedirectToAction(nameof(CustomerOverview));
        }

        return View(model);
    }

    // Allow Clients to read their assignments (Role mapping handled via display later)
    [AllowAnonymous]
    public async Task<IActionResult> CustomerOverview(int bookingId)
    {
        var assignments = await _assignmentService.GetAssignmentsByBookingIdAsync(bookingId);
        return View(assignments);
    }
}