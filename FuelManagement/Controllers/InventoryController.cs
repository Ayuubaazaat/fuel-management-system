using Microsoft.AspNetCore.Mvc;
using FuelManagement.Models;
using FuelManagement.Data;
using FuelManagement.Services.Interfaces;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using FuelManagement.Models.Enums;

namespace FuelManagement.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryController> _logger;
        private readonly INotificationService _notificationService;
        private const int PageSize = 10;

        public InventoryController(ApplicationDbContext context, ILogger<InventoryController> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        // Helper method to extract numeric part from Tank ID
        private int ExtractTankNumber(string tankId)
        {
            if (string.IsNullOrEmpty(tankId))
                return 0;

            // Use regex to find all digits in the string
            var match = Regex.Match(tankId, @"\d+");
            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            return 0;
        }

        // Helper method to check for low inventory and create alert
        private async Task CheckLowInventoryAndNotify(Inventory inventory, bool isNew = false)
        {
            var fillLevel = inventory.FillLevelPercentage;

            // Only create notification for Low or Critical levels
            if (fillLevel <= 25)
            {
                await _notificationService.CreateLowInventoryAlert(
                    tankId: inventory.TankId,
                    fuelType: inventory.FuelType,
                    currentStock: inventory.CurrentStock,
                    capacity: inventory.Capacity,
                    percentage: fillLevel,
                    actionUrl: Url.Action("Details", "Inventory", new { tankId = inventory.TankId }) // FIXED
                );
            }
        }

        // =========================
        // INDEX - WITH FIXED STATUS FILTER
        // =========================
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string tankId = null,
            string fuelType = null,
            string status = null)
        {
            var query = _context.Inventories.AsQueryable();

            // ? SAFE DATE FILTER
            if (startDate.HasValue && startDate.Value != DateTime.MinValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(i => i.LastUpdated >= start);
            }

            if (endDate.HasValue && endDate.Value != DateTime.MinValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(i => i.LastUpdated < end);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(tankId))
                query = query.Where(i => i.TankId == tankId);

            if (!string.IsNullOrEmpty(fuelType))
                query = query.Where(i => i.FuelType == fuelType);

            // Status filter - Case-sensitive but values match exactly
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Get all items first (client-side evaluation for complex ordering)
            var allItems = await query
                .Select(i => new InventoryListItemViewModel
                {
                    TankId = i.TankId,
                    FuelType = i.FuelType,
                    Capacity = i.Capacity,
                    CurrentStock = i.CurrentStock,
                    Percentage = i.FillLevelPercentage,
                    LastUpdated = i.LastUpdated,
                    Status = i.Status ?? "Available"
                })
                .ToListAsync();

            // Sort by numeric part of TankId (client-side) - FIXED to handle any prefix
            var sortedItems = allItems
                .Select(item => new
                {
                    Item = item,
                    SortOrder = ExtractTankNumber(item.TankId)
                })
                .OrderBy(x => x.SortOrder)
                .Select(x => x.Item)
                .ToList();

            // Apply pagination
            var inventoryItems = sortedItems
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Get all items for summary calculations
            var allItemsForSummary = await query
                .Select(i => new
                {
                    i.CurrentStock,
                    i.Capacity,
                    i.Status,
                    FillLevel = i.FillLevelPercentage
                })
                .ToListAsync();

            // ========== DYNAMIC PERCENTAGE CALCULATIONS ==========

            // Get total inventory (unfiltered) for baseline comparison
            var totalInventory = await _context.Inventories
                .Select(i => i.CurrentStock)
                .ToListAsync();

            var totalStockAll = totalInventory.Sum();
            var totalValueAll = totalInventory.Sum(stock => stock * 3.50m);

            // Current totals (filtered)
            var currentTotalStock = allItemsForSummary.Sum(i => i.CurrentStock);
            var currentTotalValue = allItemsForSummary.Sum(i => i.CurrentStock * 3.50m);

            // Calculate percentage of total (shows how much of total inventory is shown)
            decimal fuelStockPercentage = 0;
            if (totalStockAll > 0)
            {
                fuelStockPercentage = Math.Round((currentTotalStock / totalStockAll) * 100, 1);
            }

            decimal valuePercentage = 0;
            if (totalValueAll > 0)
            {
                valuePercentage = Math.Round((currentTotalValue / totalValueAll) * 100, 1);
            }

            // Calculate summary statistics for the view model
            var lowStockItems = allItemsForSummary.Count(i => i.Status == "Low" || i.Status == "Critical");
            var totalCapacity = allItemsForSummary.Sum(i => i.Capacity);
            var averageFillLevel = allItemsForSummary.Any() ? allItemsForSummary.Average(i => i.FillLevel) : 0;
            var inventoryValue = allItemsForSummary.Sum(i => i.CurrentStock * 3.50m);

            // ====================================================

            var filter = new InventoryFilterViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TankId = tankId,
                FuelType = fuelType,
                Status = status
            };

            // Store pagination info in ViewBag
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            var viewModel = new InventoryViewModel
            {
                Filter = filter,
                InventoryItems = inventoryItems,
                // Summary statistics
                TotalFuelStock = currentTotalStock,
                LowStockItems = lowStockItems,
                TotalCapacity = totalCapacity,
                AverageFillLevel = averageFillLevel,
                InventoryValue = inventoryValue,
                // Percentage calculations
                FuelStockGrowthPercentage = fuelStockPercentage,
                InventoryValueGrowthPercentage = valuePercentage
            };

            return View(viewModel);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            return View(new InventoryCreateViewModel());
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var existingTank = await _context.Inventories
                .FindAsync(viewModel.TankId);

            if (existingTank != null)
            {
                ModelState.AddModelError("TankId", "Tank ID already exists.");
                return View(viewModel);
            }

            var fillLevel = viewModel.Capacity > 0
                ? (viewModel.CurrentStock / viewModel.Capacity) * 100 : 0;

            string status = fillLevel switch
            {
                <= 10 => "Critical",
                <= 25 => "Low",
                _ => "Available"
            };

            var inventory = new Inventory
            {
                TankId = viewModel.TankId,
                FuelType = viewModel.FuelType,
                Capacity = viewModel.Capacity,
                CurrentStock = viewModel.CurrentStock,
                LastUpdated = DateTime.UtcNow,
                Status = status,
                LastRefillDate = viewModel.LastRefillDate,
                Notes = viewModel.Notes
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ===== CHECK FOR LOW INVENTORY ALERT =====
                await CheckLowInventoryAndNotify(inventory, isNew: true);
                // =========================================

                // ===== CREATE NOTIFICATION FOR NEW INVENTORY ITEM =====
                await _notificationService.CreateNotificationAsync(
                    title: $"New Inventory Added: {inventory.TankId}",
                    description: $"{inventory.FuelType} tank added with {inventory.CurrentStock:N0}L",
                    module: "Inventory",
                    type: NotificationType.Success,
                    actionUrl: Url.Action("Details", "Inventory", new { tankId = inventory.TankId }),
                    actionText: "View Tank"
                );
                // =====================================================

                TempData["Success"] = "Inventory item created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create inventory item");
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View(viewModel);
            }
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(string tankId)
        {
            var inventory = await _context.Inventories.FindAsync(tankId);
            if (inventory == null)
                return NotFound();

            return View(new InventoryEditViewModel
            {
                TankId = inventory.TankId,
                FuelType = inventory.FuelType,
                Capacity = inventory.Capacity,
                CurrentStock = inventory.CurrentStock,
                Status = inventory.Status,
                LastRefillDate = inventory.LastRefillDate ?? DateTime.UtcNow.Date,
                Notes = inventory.Notes
            });
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InventoryEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var inventory = await _context.Inventories.FindAsync(viewModel.TankId);
            if (inventory == null)
                return NotFound();

            if (inventory.TankId != viewModel.TankId)
            {
                ModelState.AddModelError("TankId", "Tank ID cannot be changed.");
                return View(viewModel);
            }

            var fillLevel = viewModel.Capacity > 0
                ? (viewModel.CurrentStock / viewModel.Capacity) * 100
                : 0;

            string status = fillLevel switch
            {
                <= 10 => "Critical",
                <= 25 => "Low",
                _ => "Available"
            };

            // Store old status for comparison
            var oldStatus = inventory.Status;
            var oldStock = inventory.CurrentStock;

            inventory.FuelType = viewModel.FuelType;
            inventory.Capacity = viewModel.Capacity;
            inventory.CurrentStock = viewModel.CurrentStock;
            inventory.LastUpdated = DateTime.UtcNow;
            inventory.Status = status;
            inventory.LastRefillDate = viewModel.LastRefillDate;
            inventory.Notes = viewModel.Notes;

            await _context.SaveChangesAsync();

            // ===== CHECK FOR LOW INVENTORY ALERT =====
            // Only alert if status changed to Low/Critical or if it was already Low/Critical
            if (status == "Low" || status == "Critical")
            {
                await CheckLowInventoryAndNotify(inventory);
            }
            // =========================================

            // ===== CREATE INVENTORY UPDATE NOTIFICATION =====
            string title = oldStock != viewModel.CurrentStock
                ? $"Inventory Updated: {inventory.TankId}"
                : $"Inventory Details Updated: {inventory.TankId}";

            string description = oldStock != viewModel.CurrentStock
                ? $"{inventory.FuelType} stock changed from {oldStock:N0}L to {viewModel.CurrentStock:N0}L"
                : $"{inventory.FuelType} tank information has been updated";

            await _notificationService.CreateNotificationAsync(
                title: title,
                description: description,
                module: "Inventory",
                type: NotificationType.Info,
                actionUrl: Url.Action("Details", "Inventory", new { tankId = inventory.TankId }), // FIXED
                actionText: "View Tank"
            );
            // ================================================

            TempData["Success"] = "Inventory item updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(string tankId)
        {
            var inventory = await _context.Inventories.FindAsync(tankId);
            if (inventory == null)
                return NotFound();

            return View(new InventoryListItemViewModel
            {
                TankId = inventory.TankId,
                FuelType = inventory.FuelType,
                Capacity = inventory.Capacity,
                CurrentStock = inventory.CurrentStock,
                Percentage = inventory.FillLevelPercentage,
                LastUpdated = inventory.LastUpdated,
                Status = inventory.Status
            });
        }

        // =========================
        // DELETE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string tankId)
        {
            var inventory = await _context.Inventories.FindAsync(tankId);
            if (inventory == null)
                return NotFound();

            var tankInfo = $"{inventory.TankId} - {inventory.FuelType}";

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            // ===== CREATE NOTIFICATION FOR DELETED INVENTORY =====
            await _notificationService.CreateNotificationAsync(
                title: $"Inventory Deleted: {inventory.TankId}",
                description: $"Tank {tankInfo} has been removed from inventory",
                module: "Inventory",
                type: NotificationType.Warning,
                actionUrl: Url.Action("Index", "Inventory"), // FIXED - goes to list
                actionText: "View Inventory"
            );
            // ====================================================

            TempData["Success"] = "Inventory item deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // CHECK TANK ID - Case-insensitive
        // =========================
        [HttpGet]
        public async Task<IActionResult> CheckTankId(string tankId)
        {
            if (string.IsNullOrEmpty(tankId))
                return Json(true);

            var normalizedTankId = tankId.Trim().ToUpperInvariant();
            var exists = await _context.Inventories
                .AnyAsync(i => i.TankId.ToUpper() == normalizedTankId);
            return Json(!exists);
        }
    }
}