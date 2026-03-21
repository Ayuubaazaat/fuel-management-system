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
    public class PumpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private const int PageSize = 10;

        public PumpsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Pumps
        public async Task<IActionResult> Index(PumpFilterViewModel filter, int page = 1)
        {
            // Get available filter options from database
            filter.AvailableFuelTypes = await _context.Pumps
                .Select(p => p.FuelType)
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync() ?? new List<string>();

            filter.AvailableStatuses = await _context.Pumps
                .Select(p => p.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync() ?? new List<string>();

            filter.AvailablePumps = await _context.Pumps
                .Select(p => p.PumpId)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync() ?? new List<string>();

            filter.AvailableTanks = await _context.Pumps
                .Select(p => p.TankId)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync() ?? new List<string>();

            // Build query with filters
            var query = _context.Pumps.AsQueryable();

            if (!string.IsNullOrEmpty(filter.PumpId))
                query = query.Where(p => p.PumpId == filter.PumpId);

            if (!string.IsNullOrEmpty(filter.FuelType))
                query = query.Where(p => p.FuelType == filter.FuelType);

            if (!string.IsNullOrEmpty(filter.TankId))
                query = query.Where(p => p.TankId == filter.TankId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(p => p.Status == filter.Status);

            // Only apply date filters if they are provided
            if (filter.StartDate.HasValue)
                query = query.Where(p => p.InstallationDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(p => p.InstallationDate <= filter.EndDate.Value);

            // Get total count for pagination from the filtered query
            var totalItems = await query.CountAsync();

            // Get paginated pumps from filtered query
            var pumps = await query
                .OrderBy(p => p.PumpId)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Calculate summary data from FILTERED pumps (not all pumps)
            // This ensures consistency between filters and displayed data
            var filteredPumpsList = await query.ToListAsync();

            var viewModel = new PumpsViewModel
            {
                Filter = filter,
                PumpItems = pumps.Select(p => new PumpListItemViewModel
                {
                    PumpId = p.PumpId,
                    PumpName = p.PumpName,
                    FuelType = p.FuelType,
                    TankId = p.TankId,
                    TotalFuelDispensed = p.TotalFuelDispensed,
                    FlowRate = p.FlowRate,
                    TransactionCount = p.TransactionCount,
                    Status = p.Status,
                    LastMaintenanceDate = p.LastMaintenanceDate,
                    NextMaintenanceDue = p.NextMaintenanceDue
                }).ToList(),

                // Pagination
                PageNumber = page,
                PageSize = PageSize,
                TotalItems = totalItems,

                // Summary data from FILTERED pumps (not all pumps)
                TotalPumps = filteredPumpsList.Count,
                ActivePumps = filteredPumpsList.Count(p => p.Status?.ToLower() == "active"),
                MaintenancePumps = filteredPumpsList.Count(p => p.Status?.ToLower() == "maintenance"),
                InactivePumps = filteredPumpsList.Count(p => p.Status?.ToLower() == "inactive"),
                TotalFuelDispensed = filteredPumpsList.Sum(p => p.TotalFuelDispensed),
                TotalTransactions = filteredPumpsList.Sum(p => p.TransactionCount),

                AverageFlowRate = filteredPumpsList.Where(p => p.FlowRate > 0 && p.Status?.ToLower() == "active")
                                          .Select(p => p.FlowRate)
                                          .DefaultIfEmpty(0)
                                          .Average(),

                PumpsDueForMaintenance = filteredPumpsList.Count(p =>
                    p.NextMaintenanceDue.HasValue &&
                    p.NextMaintenanceDue.Value <= DateTime.Today.AddDays(7) &&
                    p.NextMaintenanceDue.Value >= DateTime.Today),

                OverdueMaintenance = filteredPumpsList.Count(p =>
                    p.NextMaintenanceDue.HasValue &&
                    p.NextMaintenanceDue.Value < DateTime.Today)
            };

            return View(viewModel);
        }

        // GET: Pumps/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new PumpCreateViewModel
            {
                FuelTypes = await GetFuelTypesSelectList(),
                Tanks = await GetTanksSelectList(),
                StatusOptions = GetStatusSelectList()
            };

            return View(viewModel);
        }

        // POST: Pumps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PumpCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if Pump ID already exists (additional safety check)
                if (await _context.Pumps.AnyAsync(p => p.PumpId == viewModel.PumpId))
                {
                    ModelState.AddModelError("PumpId", $"Pump ID {viewModel.PumpId} is already in use.");
                }
                else
                {
                    var pump = new Pump
                    {
                        PumpId = viewModel.PumpId,
                        PumpName = viewModel.PumpName,
                        FuelType = viewModel.FuelType,
                        TankId = viewModel.TankId,
                        FlowRate = viewModel.FlowRate,
                        InstallationDate = viewModel.InstallationDate,
                        LastMaintenanceDate = viewModel.LastMaintenanceDate,
                        NextMaintenanceDue = viewModel.NextMaintenanceDue,
                        Notes = viewModel.Notes,
                        Status = viewModel.Status ?? "Active",
                        TotalFuelDispensed = 0,
                        TransactionCount = 0,
                        CreatedAt = DateTime.Now
                    };

                    _context.Pumps.Add(pump);
                    await _context.SaveChangesAsync();

                    // ===== CREATE NOTIFICATION FOR NEW PUMP =====
                    await _notificationService.CreateNotificationAsync(
                        title: $"New Pump Created: {pump.PumpId}",
                        description: $"Pump {pump.PumpName} has been added to the system",
                        module: "Pumps",
                        type: NotificationType.Success,
                        actionUrl: $"/Pumps/Details/{pump.PumpId}",
                        actionText: "View Pump"
                    );
                    // =============================================

                    TempData["Success"] = $"Pump {pump.PumpId} created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Repopulate dropdowns if validation fails
            viewModel.FuelTypes = await GetFuelTypesSelectList(viewModel.FuelType);
            viewModel.Tanks = await GetTanksSelectList(viewModel.TankId);
            viewModel.StatusOptions = GetStatusSelectList(viewModel.Status);

            return View(viewModel);
        }

        // GET: Pumps/Edit/PMP-001
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var pump = await _context.Pumps.FindAsync(id);
            if (pump == null)
                return NotFound();

            var viewModel = new PumpEditViewModel
            {
                PumpId = pump.PumpId,
                PumpName = pump.PumpName,
                FuelType = pump.FuelType,
                TankId = pump.TankId,
                FlowRate = pump.FlowRate,
                TotalFuelDispensed = pump.TotalFuelDispensed,
                TransactionCount = pump.TransactionCount,
                InstallationDate = pump.InstallationDate,
                LastMaintenanceDate = pump.LastMaintenanceDate,
                NextMaintenanceDue = pump.NextMaintenanceDue,
                Notes = pump.Notes,
                Status = pump.Status,
                UpdatedAt = pump.UpdatedAt,
                FuelTypes = await GetFuelTypesSelectList(pump.FuelType),
                Tanks = await GetTanksSelectList(pump.TankId),
                StatusOptions = GetStatusSelectList(pump.Status)
            };

            return View(viewModel);
        }

        // POST: Pumps/Edit/PMP-001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PumpEditViewModel viewModel)
        {
            if (id != viewModel.PumpId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var pump = await _context.Pumps.FindAsync(id);
                    if (pump == null)
                        return NotFound();

                    // Store old status for comparison
                    var oldStatus = pump.Status;

                    // Update pump properties
                    pump.PumpName = viewModel.PumpName;
                    pump.FuelType = viewModel.FuelType;
                    pump.TankId = viewModel.TankId;
                    pump.FlowRate = viewModel.FlowRate;
                    pump.InstallationDate = viewModel.InstallationDate;
                    pump.LastMaintenanceDate = viewModel.LastMaintenanceDate;
                    pump.NextMaintenanceDue = viewModel.NextMaintenanceDue;
                    pump.Notes = viewModel.Notes;
                    pump.Status = viewModel.Status;
                    pump.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    // ===== CHECK FOR STATUS CHANGE =====
                    if (oldStatus != viewModel.Status)
                    {
                        await _notificationService.CreatePumpStatusNotification(
                            pumpId: pump.PumpId,
                            pumpName: pump.PumpName,
                            oldStatus: oldStatus,
                            newStatus: viewModel.Status,
                            actionUrl: $"/Pumps/Details/{pump.PumpId}"
                        );
                    }
                    // ===================================

                    // ===== CHECK FOR UPCOMING MAINTENANCE =====
                    if (pump.NextMaintenanceDue.HasValue)
                    {
                        var daysUntilMaintenance = (pump.NextMaintenanceDue.Value - DateTime.Today).Days;

                        // Notify if maintenance is within 7 days
                        if (daysUntilMaintenance <= 7 && daysUntilMaintenance >= 0)
                        {
                            await _notificationService.CreatePumpMaintenanceDueNotification(
                                pumpId: pump.PumpId,
                                pumpName: pump.PumpName,
                                dueDate: pump.NextMaintenanceDue.Value,
                                actionUrl: $"/Pumps/Edit/{pump.PumpId}"
                            );
                        }
                    }
                    // =========================================

                    TempData["Success"] = $"Pump {pump.PumpId} updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PumpExists(viewModel.PumpId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Repopulate dropdowns if validation fails
            viewModel.FuelTypes = await GetFuelTypesSelectList(viewModel.FuelType);
            viewModel.Tanks = await GetTanksSelectList(viewModel.TankId);
            viewModel.StatusOptions = GetStatusSelectList(viewModel.Status);

            return View(viewModel);
        }

        // GET: Pumps/Details/PMP-001
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var pump = await _context.Pumps.FindAsync(id);
            if (pump == null)
                return NotFound();

            var viewModel = new PumpListItemViewModel
            {
                PumpId = pump.PumpId,
                PumpName = pump.PumpName,
                FuelType = pump.FuelType,
                TankId = pump.TankId,
                TotalFuelDispensed = pump.TotalFuelDispensed,
                FlowRate = pump.FlowRate,
                TransactionCount = pump.TransactionCount,
                Status = pump.Status,
                LastMaintenanceDate = pump.LastMaintenanceDate,
                NextMaintenanceDue = pump.NextMaintenanceDue
            };

            return View(viewModel);
        }

        // POST: Pumps/Delete/PMP-001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var pump = await _context.Pumps.FindAsync(id);
                if (pump != null)
                {
                    var pumpInfo = $"{pump.PumpId} - {pump.PumpName}";

                    _context.Pumps.Remove(pump);
                    await _context.SaveChangesAsync();

                    // ===== CREATE NOTIFICATION FOR DELETED PUMP =====
                    await _notificationService.CreateNotificationAsync(
                        title: $"Pump Deleted: {pump.PumpId}",
                        description: $"Pump {pump.PumpName} has been removed from the system",
                        module: "Pumps",
                        type: NotificationType.Warning,
                        actionUrl: "/Pumps",
                        actionText: "View Pumps"
                    );
                    // =================================================

                    TempData["Success"] = $"Pump {pump.PumpId} deleted successfully.";
                }
                else
                {
                    TempData["Error"] = $"Pump with ID {id} not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting pump: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // CHECK PUMP ID - With format validation
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckPumpId(string pumpId)
        {
            if (string.IsNullOrEmpty(pumpId))
                return Json(true);

            // First check format
            if (!System.Text.RegularExpressions.Regex.IsMatch(pumpId, @"^PMP-\d{3}$"))
            {
                return Json("Pump ID must be in format: PMP-001 (e.g., PMP-001, PMP-002)");
            }

            // Then check existence
            var exists = await _context.Pumps.AnyAsync(p => p.PumpId == pumpId);
            if (exists)
            {
                return Json($"Pump ID {pumpId} is already in use.");
            }

            return Json(true);
        }

        // Helper methods
        private async Task<List<SelectListItem>> GetFuelTypesSelectList(string? selected = null)
        {
            var fuelTypes = await _context.Pumps
                .Select(p => p.FuelType)
                .Distinct()
                .OrderBy(f => f)
                .ToListAsync();

            var items = fuelTypes.Select(f => new SelectListItem
            {
                Value = f,
                Text = f,
                Selected = f == selected
            }).ToList();

            // Add default options if no fuel types exist
            if (!items.Any())
            {
                items = new List<SelectListItem>
                {
                    new() { Value = "Premium Gasoline", Text = "Premium Gasoline" },
                    new() { Value = "Regular Gasoline", Text = "Regular Gasoline" },
                    new() { Value = "Diesel", Text = "Diesel" },
                    new() { Value = "Premium Diesel", Text = "Premium Diesel" },
                    new() { Value = "Ethanol", Text = "Ethanol" }
                };
            }

            return items;
        }

        private async Task<List<SelectListItem>> GetTanksSelectList(string? selected = null)
        {
            var tanks = await _context.Pumps
                .Select(p => p.TankId)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var items = tanks.Select(t => new SelectListItem
            {
                Value = t,
                Text = t,
                Selected = t == selected
            }).ToList();

            // Add default options if no tanks exist
            if (!items.Any())
            {
                items = new List<SelectListItem>
                {
                    new() { Value = "TNK-001", Text = "TNK-001 - Diesel Tank" },
                    new() { Value = "TNK-002", Text = "TNK-002 - Premium Gasoline" },
                    new() { Value = "TNK-003", Text = "TNK-003 - Regular Gasoline" },
                    new() { Value = "TNK-004", Text = "TNK-004 - Premium Diesel" },
                    new() { Value = "TNK-005", Text = "TNK-005 - Ethanol" }
                };
            }

            return items;
        }

        private List<SelectListItem> GetStatusSelectList(string? selected = null)
        {
            var statuses = new[] { "Active", "Maintenance", "Inactive" };

            return statuses.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
                Selected = s == selected
            }).ToList();
        }

        private async Task<bool> PumpExists(string id)
        {
            return await _context.Pumps.AnyAsync(p => p.PumpId == id);
        }
    }
}