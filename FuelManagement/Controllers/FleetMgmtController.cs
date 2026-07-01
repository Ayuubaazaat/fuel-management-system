using Microsoft.AspNetCore.Mvc;
using FuelManagement.Models;
using FuelManagement.Data;
using FuelManagement.Services.Interfaces;
using FuelManagement.Models.Enums;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FuelManagement.Controllers
{
    public class FleetMgmtController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FleetMgmtController> _logger;
        private readonly INotificationService _notificationService;
        private const int PageSize = 10;

        public FleetMgmtController(ApplicationDbContext context, ILogger<FleetMgmtController> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string vehicleId = null,
            string fuelType = null,
            string status = null)
        {
            var query = _context.Fleet.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(vehicleId))
                query = query.Where(f => f.VehicleId == vehicleId);

            if (!string.IsNullOrEmpty(fuelType))
                query = query.Where(f => f.FuelType == fuelType);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(f => f.Status == status);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Get paginated items
            var fleetItems = await query
                .OrderBy(f => f.VehicleId)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(f => new FleetListItemViewModel
                {
                    VehicleId = f.VehicleId,
                    VehicleName = f.VehicleName,
                    DriverName = f.DriverName,
                    LicensePlate = f.LicensePlate,
                    FuelType = f.FuelType,
                    TotalTrips = f.TotalTrips,
                    TotalFuelConsumed = f.TotalFuelConsumed,
                    FuelEfficiency = f.FuelEfficiency,
                    Status = f.Status,
                    LastServiceDate = f.LastServiceDate,
                    Odometer = f.Odometer,
                    NextServiceDue = f.NextServiceDue,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            // Get all filtered items for summary calculations
            var allFilteredItems = await query
                .Select(f => new FleetListItemViewModel
                {
                    VehicleId = f.VehicleId,
                    Status = f.Status,
                    TotalFuelConsumed = f.TotalFuelConsumed,
                    FuelEfficiency = f.FuelEfficiency,
                    NextServiceDue = f.NextServiceDue,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            // Calculate summary data
            var totalVehicles = allFilteredItems.Count;
            var activeVehicles = allFilteredItems.Count(f => f.Status == "Active");
            var maintenanceVehicles = allFilteredItems.Count(f => f.Status == "Maintenance");
            var vehiclesDueForService = allFilteredItems.Count(f => (f.NextServiceDue - DateTime.UtcNow.Date).Days <= 7);
            var totalFuelConsumption = allFilteredItems.Sum(f => f.TotalFuelConsumed);
            var avgFuelEfficiency = allFilteredItems.Any() ? allFilteredItems.Average(f => f.FuelEfficiency) : 0;
            var newVehiclesThisMonth = allFilteredItems.Count(f => f.CreatedAt >= DateTime.UtcNow.AddMonths(-1));

            var filter = new FleetFilterViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                VehicleId = vehicleId,
                FuelType = fuelType,
                Status = status
            };

            // Store pagination info in ViewBag for the view
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            var viewModel = new FleetMgmtViewModel
            {
                Filter = filter,
                FleetItems = fleetItems,
                AllFilteredItems = allFilteredItems,
                TotalVehicles = totalVehicles,
                ActiveVehicles = activeVehicles,
                MaintenanceVehicles = maintenanceVehicles,
                VehiclesDueForService = vehiclesDueForService,
                TotalFuelConsumption = totalFuelConsumption,
                AverageFuelEfficiency = avgFuelEfficiency,
                NewVehiclesThisMonth = newVehiclesThisMonth
            };

            return View(viewModel);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            return View(new FleetCreateViewModel());
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FleetCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var existingVehicle = await _context.Fleet.FindAsync(viewModel.VehicleId);

            if (existingVehicle != null)
            {
                ModelState.AddModelError("VehicleId", "Vehicle ID already exists.");
                return View(viewModel);
            }

            var fleet = new Fleet
            {
                VehicleId = viewModel.VehicleId,
                VehicleName = viewModel.VehicleName,
                DriverName = viewModel.DriverName,
                LicensePlate = viewModel.LicensePlate,
                FuelType = viewModel.FuelType,
                TotalTrips = 0,
                TotalFuelConsumed = 0,
                FuelEfficiency = 0,
                Status = viewModel.Status,
                LastServiceDate = viewModel.LastServiceDate,
                Odometer = viewModel.Odometer,
                NextServiceDue = viewModel.NextServiceDue,
                Notes = viewModel.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Fleet.Add(fleet);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ===== NOTIFICATION: New Vehicle Added =====
                await _notificationService.CreateNotificationAsync(
                    title: $"New Vehicle Added: {fleet.VehicleId}",
                    description: $"Vehicle {fleet.VehicleName} ({fleet.LicensePlate}) has been added to the fleet",
                    module: "Fleet Mgmt",
                    type: NotificationType.Success,
                    actionUrl: Url.Action("Details", "FleetMgmt", new { vehicleId = fleet.VehicleId }), // FIXED
                    actionText: "View Vehicle"
                );

                TempData["Success"] = "Vehicle added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create vehicle");
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View(viewModel);
            }
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(string vehicleId)
        {
            var fleet = await _context.Fleet.FindAsync(vehicleId);
            if (fleet == null)
                return NotFound();

            return View(new FleetEditViewModel
            {
                VehicleId = fleet.VehicleId,
                VehicleName = fleet.VehicleName,
                DriverName = fleet.DriverName,
                LicensePlate = fleet.LicensePlate,
                FuelType = fleet.FuelType,
                TotalFuelConsumed = fleet.TotalFuelConsumed,
                FuelEfficiency = fleet.FuelEfficiency,
                Status = fleet.Status,
                LastServiceDate = fleet.LastServiceDate,
                Odometer = fleet.Odometer,
                NextServiceDue = fleet.NextServiceDue,
                Notes = fleet.Notes
            });
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FleetEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var fleet = await _context.Fleet.FindAsync(viewModel.VehicleId);
            if (fleet == null)
                return NotFound();

            // VehicleId cannot be changed (primary key)
            if (fleet.VehicleId != viewModel.VehicleId)
            {
                ModelState.AddModelError("VehicleId", "Vehicle ID cannot be changed.");
                return View(viewModel);
            }

            // Store old values for notification
            var oldStatus = fleet.Status;
            var oldOdometer = fleet.Odometer;
            var oldFuelConsumed = fleet.TotalFuelConsumed;

            // Update properties
            fleet.VehicleName = viewModel.VehicleName;
            fleet.DriverName = viewModel.DriverName;
            fleet.LicensePlate = viewModel.LicensePlate;
            fleet.FuelType = viewModel.FuelType;
            fleet.TotalFuelConsumed = viewModel.TotalFuelConsumed;

            // Calculate fuel efficiency
            if (fleet.Odometer > 0 && viewModel.TotalFuelConsumed > 0)
            {
                fleet.FuelEfficiency = (viewModel.TotalFuelConsumed / fleet.Odometer) * 100;
            }

            fleet.Status = viewModel.Status;
            fleet.LastServiceDate = viewModel.LastServiceDate;
            fleet.Odometer = viewModel.Odometer;
            fleet.NextServiceDue = viewModel.NextServiceDue;
            fleet.Notes = viewModel.Notes;
            fleet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ===== NOTIFICATION: Fleet Stats Updated =====
            if (oldOdometer != viewModel.Odometer || oldFuelConsumed != viewModel.TotalFuelConsumed)
            {
                await _notificationService.CreateNotificationAsync(
                    title: $"Fleet Stats Updated: {fleet.VehicleId}",
                    description: $"Vehicle {fleet.VehicleName} stats have been updated",
                    module: "Fleet Mgmt",
                    type: NotificationType.Info,
                    actionUrl: Url.Action("Details", "FleetMgmt", new { vehicleId = fleet.VehicleId }), // FIXED
                    actionText: "View Vehicle"
                );
            }

            // ===== NOTIFICATION: Status Change =====
            if (oldStatus != viewModel.Status)
            {
                await _notificationService.CreateNotificationAsync(
                    title: $"Vehicle Status Changed: {fleet.VehicleId}",
                    description: $"Vehicle {fleet.VehicleName} status changed from {oldStatus} to {viewModel.Status}",
                    module: "Fleet Mgmt",
                    type: viewModel.Status == "Maintenance" ? NotificationType.Warning :
                          viewModel.Status == "Active" ? NotificationType.Success : NotificationType.Info,
                    actionUrl: Url.Action("Details", "FleetMgmt", new { vehicleId = fleet.VehicleId }), // FIXED
                    actionText: "View Vehicle"
                );
            }

            // ===== NOTIFICATION: Fleet Maintenance Due =====
            if (fleet.NextServiceDue <= DateTime.UtcNow.Date.AddDays(7) && fleet.NextServiceDue >= DateTime.UtcNow.Date)
            {
                var daysUntilDue = (fleet.NextServiceDue - DateTime.UtcNow.Date).Days;
                await _notificationService.CreateNotificationAsync(
                    title: $"Maintenance Due: {fleet.VehicleId}",
                    description: $"Vehicle {fleet.VehicleName} requires maintenance in {daysUntilDue} days (Due: {fleet.NextServiceDue:MMM dd, yyyy})",
                    module: "Fleet Mgmt",
                    type: daysUntilDue <= 3 ? NotificationType.Alert : NotificationType.Warning,
                    actionUrl: Url.Action("Edit", "FleetMgmt", new { vehicleId = fleet.VehicleId }), // FIXED
                    actionText: "Schedule Now"
                );
            }

            TempData["Success"] = "Vehicle updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(string vehicleId)
        {
            var fleet = await _context.Fleet.FindAsync(vehicleId);
            if (fleet == null)
                return NotFound();

            return View(new FleetListItemViewModel
            {
                VehicleId = fleet.VehicleId,
                VehicleName = fleet.VehicleName,
                DriverName = fleet.DriverName,
                LicensePlate = fleet.LicensePlate,
                FuelType = fleet.FuelType,
                TotalTrips = fleet.TotalTrips,
                TotalFuelConsumed = fleet.TotalFuelConsumed,
                FuelEfficiency = fleet.FuelEfficiency,
                Status = fleet.Status,
                LastServiceDate = fleet.LastServiceDate,
                Odometer = fleet.Odometer,
                NextServiceDue = fleet.NextServiceDue,
                CreatedAt = fleet.CreatedAt
            });
        }

        // =========================
        // DELETE (GET) - Shows confirmation page
        // =========================
        [HttpGet]
        public async Task<IActionResult> Delete(string vehicleId)
        {
            if (string.IsNullOrEmpty(vehicleId))
                return NotFound();

            var fleet = await _context.Fleet.FindAsync(vehicleId);
            if (fleet == null)
                return NotFound();

            var viewModel = new FleetListItemViewModel
            {
                VehicleId = fleet.VehicleId,
                VehicleName = fleet.VehicleName,
                DriverName = fleet.DriverName,
                LicensePlate = fleet.LicensePlate,
                FuelType = fleet.FuelType,
                TotalTrips = fleet.TotalTrips,
                TotalFuelConsumed = fleet.TotalFuelConsumed,
                FuelEfficiency = fleet.FuelEfficiency,
                Status = fleet.Status,
                LastServiceDate = fleet.LastServiceDate,
                Odometer = fleet.Odometer,
                NextServiceDue = fleet.NextServiceDue,
                CreatedAt = fleet.CreatedAt
            };

            return View(viewModel);
        }

        // =========================
        // DELETE (POST) - Processes the deletion
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string vehicleId)
        {
            if (string.IsNullOrEmpty(vehicleId))
                return NotFound();

            var fleet = await _context.Fleet.FindAsync(vehicleId);
            if (fleet == null)
                return NotFound();

            _context.Fleet.Remove(fleet);
            await _context.SaveChangesAsync();

            // ===== NOTIFICATION: Vehicle Deleted =====
            await _notificationService.CreateNotificationAsync(
                title: $"Vehicle Deleted: {fleet.VehicleId}",
                description: $"Vehicle {fleet.VehicleName} ({fleet.LicensePlate}) has been removed from the fleet",
                module: "Fleet Mgmt",
                type: NotificationType.Warning,
                actionUrl: Url.Action("Index", "FleetMgmt"), // FIXED
                actionText: "View Fleet"
            );

            TempData["Success"] = "Vehicle deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // CHECK VEHICLE ID - With format validation
        // =========================
        [HttpGet]
        public async Task<IActionResult> CheckVehicleId(string vehicleId)
        {
            if (string.IsNullOrEmpty(vehicleId))
                return Json(true);

            // First check format
            if (!System.Text.RegularExpressions.Regex.IsMatch(vehicleId, @"^VH-\d+$"))
            {
                // Return the error message directly - this will show in the validation summary
                return Json("Vehicle ID must be in format: VH-001 (e.g., VH-001, VH-002)");
            }

            // Then check existence
            var exists = await _context.Fleet.AnyAsync(f => f.VehicleId == vehicleId);
            if (exists)
            {
                return Json("Vehicle ID already exists");
            }

            return Json(true);
        }
        // =========================
        // CHECK LICENSE PLATE - Case insensitive
        // =========================
        [HttpGet]
        public async Task<IActionResult> CheckLicensePlate(string licensePlate)
        {
            if (string.IsNullOrEmpty(licensePlate))
                return Json(true);

            var exists = await _context.Fleet
                .AnyAsync(f => f.LicensePlate.ToUpper() == licensePlate.ToUpper());
            return Json(!exists);
        }
    }
}