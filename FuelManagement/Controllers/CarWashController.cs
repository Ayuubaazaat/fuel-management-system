using FuelManagement.Data;
using FuelManagement.Models;
using FuelManagement.Services;
using FuelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuelManagement.Controllers
{
    [Authorize]
    public class CarWashController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly INotificationService _notificationService;

        public CarWashController(
            ApplicationDbContext context,
            AuthService authService,
            INotificationService notificationService)
        {
            _context = context;
            _authService = authService;
            _notificationService = notificationService;
        }

        // =============================================
        // INDEX
        // =============================================
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? statusFilter,
            string? serviceFilter,
            string? paymentFilter,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var query = _context.CarWashes.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(c =>
                    c.CustomerName.Contains(searchTerm) ||
                    c.VehiclePlate.Contains(searchTerm) ||
                    (c.AssignedTo != null && c.AssignedTo.Contains(searchTerm)));

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(c => c.Status == statusFilter);

            if (!string.IsNullOrEmpty(serviceFilter))
                query = query.Where(c => c.ServiceType == serviceFilter);

            if (!string.IsNullOrEmpty(paymentFilter))
                query = query.Where(c => c.PaymentStatus == paymentFilter);

            if (dateFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(c => c.CreatedAt <= dateTo.Value.AddDays(1));

            var records = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var allRecords = await _context.CarWashes.ToListAsync();

            ViewBag.TotalWashes = allRecords.Count;
            ViewBag.TotalRevenue = allRecords.Where(c => c.PaymentStatus == "Paid").Sum(c => c.Price);
            ViewBag.TodayWashes = allRecords.Count(c => c.CreatedAt.Date == DateTime.UtcNow.Date);
            ViewBag.TodayRevenue = allRecords.Where(c => c.CreatedAt.Date == DateTime.UtcNow.Date && c.PaymentStatus == "Paid").Sum(c => c.Price);
            ViewBag.PendingCount = allRecords.Count(c => c.Status == "Pending");
            ViewBag.InProgressCount = allRecords.Count(c => c.Status == "InProgress");
            ViewBag.CompletedCount = allRecords.Count(c => c.Status == "Completed");
            ViewBag.CancelledCount = allRecords.Count(c => c.Status == "Cancelled");
            ViewBag.UnpaidCount = allRecords.Count(c => c.PaymentStatus == "Unpaid");

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.ServiceFilter = serviceFilter;
            ViewBag.PaymentFilter = paymentFilter;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(records);
        }

        // =============================================
        // DETAILS
        // =============================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var carWash = await _context.CarWashes.FirstOrDefaultAsync(c => c.Id == id);
            if (carWash == null) return NotFound();
            return View(carWash);
        }

        // =============================================
        // CREATE - GET
        // =============================================
        public IActionResult Create()
        {
            var model = new CarWash
            {
                CreatedBy = User.Identity?.Name ?? "Unknown",
                CreatedAt = DateTime.UtcNow,
                PaymentMethod = "EVC+"
            };
            return View(model);
        }

        // =============================================
        // CREATE - POST
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarWash model)
        {
            // Plate uniqueness check
            var plateExists = await _context.CarWashes
                .AnyAsync(c => c.VehiclePlate.ToUpper() == model.VehiclePlate.ToUpper());

            if (plateExists)
                ModelState.AddModelError("VehiclePlate",
                    $"Vehicle plate '{model.VehiclePlate.ToUpper()}' already has an active record. Use Edit to update it.");

            if (ModelState.IsValid)
            {
                model.VehiclePlate = model.VehiclePlate.ToUpper();
                model.CreatedAt = DateTime.UtcNow;
                model.CreatedBy = User.Identity?.Name ?? "Unknown";

                if (model.Status == "Completed")
                    model.CompletedAt = DateTime.UtcNow;

                _context.CarWashes.Add(model);
                await _context.SaveChangesAsync();

                // Fire created notification
                var actionUrl = Url.Action("Details", "CarWash", new { id = model.Id }) ?? "/CarWash";
                await _notificationService.CreateCarWashCreatedNotification(
                    model.Id, model.CustomerName, model.VehiclePlate,
                    model.ServiceType, model.Price, actionUrl);

                // If already completed on creation, fire completed notification too
                if (model.Status == "Completed")
                    await _notificationService.CreateCarWashCompletedNotification(
                        model.Id, model.CustomerName, model.VehiclePlate,
                        model.Price, actionUrl);

                TempData["Success"] = $"Car wash record for {model.CustomerName} ({model.VehiclePlate}) created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // =============================================
        // EDIT - GET
        // =============================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var carWash = await _context.CarWashes.FindAsync(id);
            if (carWash == null) return NotFound();
            return View(carWash);
        }

        // =============================================
        // EDIT - POST
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarWash model)
        {
            if (id != model.Id) return NotFound();

            // Plate uniqueness — skip own record
            var plateExists = await _context.CarWashes
                .AnyAsync(c => c.VehiclePlate.ToUpper() == model.VehiclePlate.ToUpper()
                            && c.Id != model.Id);

            if (plateExists)
                ModelState.AddModelError("VehiclePlate",
                    $"Vehicle plate '{model.VehiclePlate.ToUpper()}' is already used by another record.");

            if (ModelState.IsValid)
            {
                try
                {
                    model.VehiclePlate = model.VehiclePlate.ToUpper();

                    var previousStatus = await _context.CarWashes
                        .AsNoTracking()
                        .Where(c => c.Id == id)
                        .Select(c => c.Status)
                        .FirstOrDefaultAsync();

                    if (model.Status == "Completed" && model.CompletedAt == null)
                        model.CompletedAt = DateTime.UtcNow;

                    if (model.Status != "Completed")
                        model.CompletedAt = null;

                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    var actionUrl = Url.Action("Details", "CarWash", new { id = model.Id }) ?? "/CarWash";

                    // Fire completed notification if status just changed to Completed
                    if (model.Status == "Completed" && previousStatus != "Completed")
                        await _notificationService.CreateCarWashCompletedNotification(
                            model.Id, model.CustomerName, model.VehiclePlate,
                            model.Price, actionUrl);
                    else
                        await _notificationService.CreateCarWashUpdatedNotification(
                            model.Id, model.CustomerName, model.VehiclePlate, actionUrl);

                    TempData["Success"] = $"Car wash record for {model.CustomerName} ({model.VehiclePlate}) updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarWashExists(model.Id)) return NotFound();
                    throw;
                }
            }

            return View(model);
        }

        // =============================================
        // DELETE - POST (Admin only)
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_authService.IsAdmin())
            {
                TempData["Error"] = "Only administrators can delete car wash records.";
                return RedirectToAction(nameof(Index));
            }

            var carWash = await _context.CarWashes.FindAsync(id);
            if (carWash == null) return NotFound();

            var customerName = carWash.CustomerName;
            var vehiclePlate = carWash.VehiclePlate;
            var washId = carWash.Id;

            _context.CarWashes.Remove(carWash);
            await _context.SaveChangesAsync();

            // Fire delete notification — points to index since record is gone
            var actionUrl = Url.Action("Index", "CarWash") ?? "/CarWash";
            await _notificationService.CreateCarWashDeletedNotification(
                washId, customerName, vehiclePlate, actionUrl);

            TempData["Success"] = $"Car wash record for {customerName} ({vehiclePlate}) deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // UPDATE STATUS - AJAX
        // =============================================
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var carWash = await _context.CarWashes.FindAsync(id);
            if (carWash == null)
                return Json(new { success = false, message = "Record not found." });

            var validStatuses = new[] { "Pending", "InProgress", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
                return Json(new { success = false, message = "Invalid status." });

            var previousStatus = carWash.Status;
            carWash.Status = status;

            if (status == "Completed")
                carWash.CompletedAt = DateTime.UtcNow;
            else
                carWash.CompletedAt = null;

            await _context.SaveChangesAsync();

            // Fire completed notification if just marked done
            if (status == "Completed" && previousStatus != "Completed")
            {
                var actionUrl = Url.Action("Details", "CarWash", new { id }) ?? "/CarWash";
                await _notificationService.CreateCarWashCompletedNotification(
                    carWash.Id, carWash.CustomerName, carWash.VehiclePlate,
                    carWash.Price, actionUrl);
            }

            return Json(new { success = true, message = $"Status updated to {status}." });
        }

        // =============================================
        // UPDATE PAYMENT - AJAX
        // =============================================
        [HttpPost]
        public async Task<IActionResult> UpdatePayment(int id, string paymentStatus)
        {
            var carWash = await _context.CarWashes.FindAsync(id);
            if (carWash == null)
                return Json(new { success = false, message = "Record not found." });

            carWash.PaymentStatus = paymentStatus;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Payment updated to {paymentStatus}." });
        }

        // =============================================
        // VALIDATE PLATE - Remote validation endpoint
        // =============================================
        [HttpGet]
        public async Task<IActionResult> ValidatePlate(string vehiclePlate, int? id)
        {
            if (string.IsNullOrEmpty(vehiclePlate))
                return Json(true);

            var plate = vehiclePlate.ToUpper();
            var exists = await _context.CarWashes
                .AnyAsync(c => c.VehiclePlate.ToUpper() == plate
                            && (id == null || c.Id != id));

            return exists
                ? Json($"Vehicle plate '{plate}' already has an active record.")
                : Json(true);
        }

        // =============================================
        // PRIVATE HELPER
        // =============================================
        private bool CarWashExists(int id) =>
            _context.CarWashes.Any(c => c.Id == id);
    }
}