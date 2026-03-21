using Microsoft.AspNetCore.Mvc;
using FuelManagement.Models;
using FuelManagement.Data;
using FuelManagement.Services.Interfaces;
using FuelManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FuelManagement.Controllers
{
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private const int PageSize = 10;

        public TripsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // =========================
        // INDEX - List all trips with filters and pagination
        // =========================
        public async Task<IActionResult> Index(TripFilterViewModel filter, int page = 1)
        {
            // Get available filter options from database
            filter.AvailableVehicles = await _context.Trips
                .Select(t => t.VehicleName)
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync() ?? new List<string>();

            filter.AvailableDrivers = await _context.Trips
                .Select(t => t.DriverName)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync() ?? new List<string>();

            // Build query with filters
            var query = _context.Trips.AsQueryable();

            // Apply date filters
            if (filter.StartDate.HasValue)
                query = query.Where(t => t.TripDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(t => t.TripDate <= filter.EndDate.Value);

            // Apply text filters
            if (!string.IsNullOrEmpty(filter.TripId))
                query = query.Where(t => t.TripId.Contains(filter.TripId));

            if (!string.IsNullOrEmpty(filter.DriverName))
                query = query.Where(t => t.DriverName.Contains(filter.DriverName));

            // Apply numeric range filters
            if (filter.MinDistance.HasValue)
                query = query.Where(t => t.Distance >= filter.MinDistance.Value);

            if (filter.MaxDistance.HasValue)
                query = query.Where(t => t.Distance <= filter.MaxDistance.Value);

            if (filter.MinFuelUsed.HasValue)
                query = query.Where(t => t.FuelUsed >= filter.MinFuelUsed.Value);

            if (filter.MaxFuelUsed.HasValue)
                query = query.Where(t => t.FuelUsed <= filter.MaxFuelUsed.Value);

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Get paginated trips
            var trips = await query
                .OrderBy(t => t.TripId)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Calculate summary data from FILTERED trips
            var filteredTripsList = await query.ToListAsync();

            var viewModel = new TripsViewModel
            {
                Filter = filter,
                TripItems = trips.Select(t => new TripListItemViewModel
                {
                    TripId = t.TripId,
                    VehicleName = t.VehicleName,
                    LicensePlate = t.LicensePlate,
                    DriverName = t.DriverName,
                    TripDate = t.TripDate,
                    Distance = t.Distance,
                    FuelUsed = t.FuelUsed,
                    FuelEfficiency = t.FuelEfficiency,
                    Cost = t.Cost,
                    Notes = t.Notes
                }).ToList(),

                // Pagination
                PageNumber = page,
                PageSize = PageSize,
                TotalItems = totalItems,

                // Summary data from FILTERED trips
                TotalDistance = filteredTripsList.Sum(t => t.Distance),
                TotalFuelUsed = filteredTripsList.Sum(t => t.FuelUsed),
                TotalCost = filteredTripsList.Sum(t => t.Cost),
                AverageEfficiency = filteredTripsList.Where(t => t.FuelEfficiency > 0)
                                                     .Select(t => t.FuelEfficiency)
                                                     .DefaultIfEmpty(0)
                                                     .Average(),
                AverageDistance = filteredTripsList.Any()
                                 ? filteredTripsList.Average(t => t.Distance)
                                 : 0,
                AverageCostPerTrip = filteredTripsList.Any()
                                    ? filteredTripsList.Average(t => t.Cost)
                                    : 0,
                NewTripsThisMonth = filteredTripsList.Count(t =>
                    t.TripDate >= DateTime.Today.AddMonths(-1))
            };

            // Store pagination info in ViewBag for the view
            ViewBag.PageNumber = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / PageSize);

            return View(viewModel);
        }

        // =========================
        // CREATE (GET) - WITH VEHICLE DROPDOWN
        // =========================
        public async Task<IActionResult> Create()
        {
            var viewModel = new TripCreateViewModel
            {
                TripDate = DateTime.Today,
                AvailableVehicles = await _context.Fleet
                    .OrderBy(f => f.VehicleId)
                    .Select(f => new SelectListItem
                    {
                        Value = f.VehicleId,
                        Text = $"{f.VehicleId} - {f.VehicleName} ({f.LicensePlate})"
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // =========================
        // CREATE (POST) - WITH FLEET UPDATE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if Trip ID already exists
                if (await _context.Trips.AnyAsync(t => t.TripId == viewModel.TripId))
                {
                    ModelState.AddModelError("TripId", $"Trip ID {viewModel.TripId} already exists.");
                }
                // Check if License Plate already exists
                else if (await _context.Trips.AnyAsync(t => t.LicensePlate == viewModel.LicensePlate))
                {
                    ModelState.AddModelError("LicensePlate", $"License plate {viewModel.LicensePlate} already exists.");
                }
                else
                {
                    // Calculate fuel efficiency
                    var fuelEfficiency = viewModel.Distance > 0
                        ? (viewModel.FuelUsed / viewModel.Distance) * 100
                        : 0;

                    var trip = new Trip
                    {
                        TripId = viewModel.TripId,
                        VehicleId = viewModel.VehicleId,
                        VehicleName = viewModel.VehicleName,
                        LicensePlate = viewModel.LicensePlate,
                        DriverName = viewModel.DriverName,
                        TripDate = viewModel.TripDate,
                        Distance = viewModel.Distance,
                        FuelUsed = viewModel.FuelUsed,
                        FuelEfficiency = Math.Round(fuelEfficiency, 1),
                        Cost = viewModel.Cost,
                        Notes = viewModel.Notes,
                        CreatedAt = DateTime.Now
                    };

                    // Start a transaction to ensure both operations succeed or fail together
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Trips.Add(trip);
                        await _context.SaveChangesAsync();

                        // UPDATE FLEET STATISTICS
                        await UpdateFleetStatistics(viewModel.VehicleId, trip.Distance, trip.FuelUsed, true);

                        await transaction.CommitAsync();

                        // ===== CREATE NOTIFICATION FOR TRIP COMPLETED =====
                        await _notificationService.CreateTripCompletedNotification(
                            tripId: trip.TripId,
                            vehicleName: trip.VehicleName,
                            driverName: trip.DriverName,
                            distance: trip.Distance,
                            fuelUsed: trip.FuelUsed,
                            actionUrl: $"/Trips/Details/{trip.TripId}"
                        );

                        // ===== CHECK FOR FUEL EFFICIENCY ALERT =====
                        // Get average fuel efficiency for this vehicle from past trips
                        var vehicleTrips = await _context.Trips
                            .Where(t => t.VehicleId == trip.VehicleId && t.TripId != trip.TripId)
                            .Select(t => t.FuelEfficiency)
                            .ToListAsync();

                        if (vehicleTrips.Any())
                        {
                            var avgEfficiency = vehicleTrips.Average();

                            // If current trip is 20% less efficient than average
                            if (trip.FuelEfficiency > avgEfficiency * 1.2m)
                            {
                                await _notificationService.CreateTripAlertNotification(
                                    tripId: trip.TripId,
                                    vehicleName: trip.VehicleName,
                                    alertType: "Fuel Efficiency",
                                    description: $"Trip showed {((trip.FuelEfficiency - avgEfficiency) / avgEfficiency * 100):F0}% lower fuel efficiency than average ({trip.FuelEfficiency:F1}L/100km vs avg {avgEfficiency:F1}L/100km)",
                                    actionUrl: $"/Trips/Details/{trip.TripId}"
                                );
                            }
                        }
                        // =============================================

                        TempData["Success"] = $"Trip {trip.TripId} logged successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                    }
                }
            }

            // Repopulate dropdown if validation fails
            viewModel.AvailableVehicles = await _context.Fleet
                .OrderBy(f => f.VehicleId)
                .Select(f => new SelectListItem
                {
                    Value = f.VehicleId,
                    Text = $"{f.VehicleId} - {f.VehicleName} ({f.LicensePlate})"
                })
                .ToListAsync();

            return View(viewModel);
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
                return NotFound();

            var viewModel = new TripEditViewModel
            {
                TripId = trip.TripId,
                VehicleId = trip.VehicleId,
                VehicleName = trip.VehicleName,
                LicensePlate = trip.LicensePlate,
                DriverName = trip.DriverName,
                TripDate = trip.TripDate,
                Distance = trip.Distance,
                FuelUsed = trip.FuelUsed,
                FuelEfficiency = trip.FuelEfficiency,
                Cost = trip.Cost,
                Notes = trip.Notes,
                UpdatedAt = trip.UpdatedAt,
                AvailableVehicles = await _context.Fleet
                    .OrderBy(f => f.VehicleId)
                    .Select(f => new SelectListItem
                    {
                        Value = f.VehicleId,
                        Text = $"{f.VehicleId} - {f.VehicleName} ({f.LicensePlate})",
                        Selected = f.VehicleId == trip.VehicleId
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // =========================
        // EDIT (POST) - WITH FLEET UPDATE - FIXED VERSION
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TripEditViewModel viewModel)
        {
            if (id != viewModel.TripId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var trip = await _context.Trips.FindAsync(id);
                    if (trip == null)
                        return NotFound();

                    // Check if License Plate is being changed and already exists
                    if (trip.LicensePlate != viewModel.LicensePlate &&
                        await _context.Trips.AnyAsync(t => t.LicensePlate == viewModel.LicensePlate && t.TripId != id))
                    {
                        ModelState.AddModelError("LicensePlate", $"License plate {viewModel.LicensePlate} already exists.");

                        // Repopulate dropdown
                        viewModel.AvailableVehicles = await _context.Fleet
                            .OrderBy(f => f.VehicleId)
                            .Select(f => new SelectListItem
                            {
                                Value = f.VehicleId,
                                Text = $"{f.VehicleId} - {f.VehicleName} ({f.LicensePlate})"
                            })
                            .ToListAsync();

                        return View(viewModel);
                    }

                    // Store old values for fleet update
                    var oldDistance = trip.Distance;
                    var oldFuelUsed = trip.FuelUsed;
                    var oldVehicleId = trip.VehicleId;

                    // Calculate new fuel efficiency
                    var fuelEfficiency = viewModel.Distance > 0
                        ? (viewModel.FuelUsed / viewModel.Distance) * 100
                        : 0;

                    // Update trip properties
                    trip.VehicleId = viewModel.VehicleId;
                    trip.VehicleName = viewModel.VehicleName;
                    trip.LicensePlate = viewModel.LicensePlate;
                    trip.DriverName = viewModel.DriverName;
                    trip.TripDate = viewModel.TripDate;
                    trip.Distance = viewModel.Distance;
                    trip.FuelUsed = viewModel.FuelUsed;
                    trip.FuelEfficiency = Math.Round(fuelEfficiency, 1);
                    trip.Cost = viewModel.Cost;
                    trip.Notes = viewModel.Notes;
                    trip.UpdatedAt = DateTime.Now;

                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        await _context.SaveChangesAsync();

                        // UPDATE FLEET STATISTICS - FIXED LOGIC
                        if (oldVehicleId != viewModel.VehicleId)
                        {
                            // Vehicle changed: remove from old, add to new
                            await UpdateFleetStatistics(oldVehicleId, -oldDistance, -oldFuelUsed, isAdd: false, isEdit: true);
                            await UpdateFleetStatistics(viewModel.VehicleId, viewModel.Distance, viewModel.FuelUsed, isAdd: true, isEdit: true);
                        }
                        else
                        {
                            // Same vehicle: update with net change (trip count stays the same)
                            var distanceChange = viewModel.Distance - oldDistance;
                            var fuelChange = viewModel.FuelUsed - oldFuelUsed;

                            if (distanceChange != 0 || fuelChange != 0)
                            {
                                await UpdateFleetStatistics(viewModel.VehicleId, distanceChange, fuelChange, isAdd: false, isEdit: true);
                            }
                        }

                        await transaction.CommitAsync();

                        // ===== CREATE NOTIFICATION FOR TRIP UPDATED =====
                        await _notificationService.CreateNotificationAsync(
                            title: $"Trip Updated: {trip.TripId}",
                            description: $"Trip for {trip.VehicleName} has been updated",
                            module: "Trips",
                            type: NotificationType.Info,
                            actionUrl: $"/Trips/Details/{trip.TripId}",
                            actionText: "View Trip"
                        );
                        // =================================================

                        TempData["Success"] = $"Trip {trip.TripId} updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "An error occurred while updating. Please try again.");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TripExists(viewModel.TripId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Repopulate dropdown if validation fails
            viewModel.AvailableVehicles = await _context.Fleet
                .OrderBy(f => f.VehicleId)
                .Select(f => new SelectListItem
                {
                    Value = f.VehicleId,
                    Text = $"{f.VehicleId} - {f.VehicleName} ({f.LicensePlate})"
                })
                .ToListAsync();

            return View(viewModel);
        }

        // =========================
        // DETAILS (GET)
        // =========================
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
                return NotFound();

            var viewModel = new TripListItemViewModel
            {
                TripId = trip.TripId,
                VehicleName = trip.VehicleName,
                LicensePlate = trip.LicensePlate,
                DriverName = trip.DriverName,
                TripDate = trip.TripDate,
                Distance = trip.Distance,
                FuelUsed = trip.FuelUsed,
                FuelEfficiency = trip.FuelEfficiency,
                Cost = trip.Cost,
                Notes = trip.Notes
            };

            return View(viewModel);
        }

        // =========================
        // DELETE (POST) - WITH FLEET UPDATE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var trip = await _context.Trips.FindAsync(id);
                if (trip != null)
                {
                    // Store values for fleet update before deleting
                    var vehicleId = trip.VehicleId;
                    var distance = trip.Distance;
                    var fuelUsed = trip.FuelUsed;

                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Trips.Remove(trip);
                        await _context.SaveChangesAsync();

                        // UPDATE FLEET STATISTICS - Subtract the deleted trip
                        await UpdateFleetStatistics(vehicleId, -distance, -fuelUsed, isAdd: false, isEdit: false);

                        await transaction.CommitAsync();

                        // ===== CREATE NOTIFICATION FOR TRIP DELETED =====
                        await _notificationService.CreateNotificationAsync(
                            title: $"Trip Deleted: {trip.TripId}",
                            description: $"Trip for {trip.VehicleName} has been deleted",
                            module: "Trips",
                            type: NotificationType.Warning,
                            actionUrl: "/Trips",
                            actionText: "View Trips"
                        );
                        // =================================================

                        TempData["Success"] = $"Trip {trip.TripId} deleted successfully.";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Error deleting trip. Please try again.";
                    }
                }
                else
                {
                    TempData["Error"] = $"Trip with ID {id} not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting trip: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // REMOTE VALIDATION - Check if Trip ID exists
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckTripId(string tripId)
        {
            var exists = await _context.Trips.AnyAsync(t => t.TripId == tripId);
            if (exists)
                return Json($"Trip ID {tripId} is already in use.");

            return Json(true);
        }

        // =========================
        // REMOTE VALIDATION - Check if License Plate exists
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckLicensePlate(string licensePlate)
        {
            var exists = await _context.Trips.AnyAsync(t => t.LicensePlate == licensePlate);
            if (exists)
                return Json($"License plate {licensePlate} already exists.");

            return Json(true);
        }

        // =========================
        // GET VEHICLE DETAILS - For AJAX auto-fill
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetVehicleDetails(string vehicleId)
        {
            var vehicle = await _context.Fleet
                .Where(f => f.VehicleId == vehicleId)
                .Select(f => new {
                    f.VehicleName,
                    f.LicensePlate,
                    f.DriverName
                })
                .FirstOrDefaultAsync();

            if (vehicle == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                vehicleName = vehicle.VehicleName,
                licensePlate = vehicle.LicensePlate,
                driverName = vehicle.DriverName
            });
        }

        // =========================
        // HELPER METHOD - Update Fleet Statistics - FIXED VERSION
        // =========================
        private async Task UpdateFleetStatistics(string vehicleId, decimal distanceChange, decimal fuelChange, bool isAdd, bool isEdit = false)
        {
            var fleetVehicle = await _context.Fleet.FindAsync(vehicleId);

            if (fleetVehicle != null)
            {
                // Update trip count - ONLY for new trips (isAdd=true) or deleted trips (isAdd=false with negative values)
                if (isAdd && !isEdit)
                {
                    // New trip being added
                    fleetVehicle.TotalTrips++;
                }
                else if (!isAdd && !isEdit && distanceChange < 0 && fuelChange < 0)
                {
                    // Trip being deleted
                    fleetVehicle.TotalTrips--;
                }
                // For edits, trip count stays the same

                // Ensure trip count doesn't go negative
                if (fleetVehicle.TotalTrips < 0)
                    fleetVehicle.TotalTrips = 0;

                // Update odometer and fuel consumed
                fleetVehicle.Odometer += (int)distanceChange;
                fleetVehicle.TotalFuelConsumed += fuelChange;

                // Ensure values don't go negative
                if (fleetVehicle.Odometer < 0) fleetVehicle.Odometer = 0;
                if (fleetVehicle.TotalFuelConsumed < 0) fleetVehicle.TotalFuelConsumed = 0;

                // Recalculate fuel efficiency
                if (fleetVehicle.Odometer > 0)
                {
                    fleetVehicle.FuelEfficiency = Math.Round(
                        (fleetVehicle.TotalFuelConsumed / fleetVehicle.Odometer) * 100, 1
                    );
                }
                else
                {
                    fleetVehicle.FuelEfficiency = 0;
                }

                fleetVehicle.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> TripExists(string id)
        {
            return await _context.Trips.AnyAsync(t => t.TripId == id);
        }
    }
}