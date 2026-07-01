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
    public class CompensationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private const int PageSize = 10;

        public CompensationController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // =========================
        // INDEX - List all compensations with filters and pagination
        // =========================
        public async Task<IActionResult> Index(CompensationFilterViewModel filter, int page = 1)
        {
            // Get available filter options from database
            filter.AvailableDepartments = await _context.Compensations
                .Select(c => c.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync() ?? new List<string>();

            filter.AvailableStatuses = await _context.Compensations
                .Select(c => c.PaymentStatus)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync() ?? new List<string>();

            filter.AvailableEmployees = await _context.Compensations
                .Select(c => c.EmployeeId)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync() ?? new List<string>();

            // Build query with filters
            var query = _context.Compensations.AsQueryable();

            // Apply date filters
            if (filter.StartDate.HasValue)
                query = query.Where(c => c.PaymentDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.PaymentDate <= filter.EndDate.Value);

            // Apply text filters
            if (!string.IsNullOrEmpty(filter.EmployeeId))
                query = query.Where(c => c.EmployeeId == filter.EmployeeId);

            if (!string.IsNullOrEmpty(filter.Department))
                query = query.Where(c => c.Department == filter.Department);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(c => c.PaymentStatus == filter.Status);

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // FIXED: Order by EmployeeId numeric value FIRST, THEN apply pagination
            // This ensures correct ordering across all pages (001,002,003...010,011,012)
            var allCompensations = await query
                .ToListAsync();

            // Order all records by numeric part of EmployeeId
            var orderedCompensations = allCompensations
                .OrderBy(c => int.Parse(c.EmployeeId.Replace("EMP-", "")))
                .ToList();

            // Apply pagination to the ordered list
            var pagedCompensations = orderedCompensations
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Calculate summary data from FILTERED compensations (ALL records, not just current page)
            var allFilteredList = allCompensations; // Use all records for summary cards

            var viewModel = new CompensationViewModel
            {
                Filter = filter,
                CompensationItems = pagedCompensations.Select(c => new CompensationListItemViewModel
                {
                    Id = c.Id,
                    EmployeeId = c.EmployeeId,
                    EmployeeName = c.EmployeeName,
                    PhoneNumber = c.PhoneNumber,
                    Position = c.Position,
                    Department = c.Department,
                    BaseSalary = c.BaseSalary,
                    Bonus = c.Bonus,
                    Deductions = c.Deductions,
                    NetSalary = c.NetSalary,
                    PaymentDate = c.PaymentDate,
                    PaymentMethod = c.PaymentMethod,
                    Status = c.PaymentStatus,
                    Notes = c.Notes
                }).ToList(),

                // Summary data from ALL FILTERED compensations (not just current page)
                TotalPayroll = allFilteredList.Sum(c => c.NetSalary),
                TotalPaid = allFilteredList.Where(c => c.PaymentStatus == "Paid").Sum(c => c.NetSalary),
                TotalPending = allFilteredList.Where(c => c.PaymentStatus == "Pending").Sum(c => c.NetSalary),
                TotalOverdue = allFilteredList.Where(c => c.PaymentStatus == "Overdue").Sum(c => c.NetSalary),
                PaidCount = allFilteredList.Count(c => c.PaymentStatus == "Paid"),
                PendingCount = allFilteredList.Count(c => c.PaymentStatus == "Pending"),
                OverdueCount = allFilteredList.Count(c => c.PaymentStatus == "Overdue"),
                AverageSalary = allFilteredList.Any() ? allFilteredList.Average(c => c.BaseSalary) : 0,
                TotalBonuses = allFilteredList.Sum(c => c.Bonus),
                TotalDeductions = allFilteredList.Sum(c => c.Deductions)
            };

            // Store pagination info in ViewBag
            ViewBag.PageNumber = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / PageSize);

            return View(viewModel);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            var viewModel = new CompensationCreateViewModel
            {
                PaymentDate = DateTime.UtcNow.Date
            };
            return View(viewModel);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompensationCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if Employee ID already exists (additional server-side check)
                var existingEmployee = await _context.Compensations
                    .FirstOrDefaultAsync(c => c.EmployeeId == viewModel.EmployeeId);

                if (existingEmployee != null)
                {
                    ModelState.AddModelError("EmployeeId", "Employee ID already exists");
                    return View(viewModel);
                }

                // Check if Phone Number already exists (server-side guard — uniqueness enforced here)
                var existingPhone = await _context.Compensations
                    .FirstOrDefaultAsync(c => c.PhoneNumber == viewModel.PhoneNumber);

                if (existingPhone != null)
                {
                    ModelState.AddModelError("PhoneNumber", "Phone number already exists");
                    return View(viewModel);
                }

                // Generate Compensation ID (e.g., COMP-001, COMP-002)
                var lastCompensation = await _context.Compensations
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                int newId = 1;
                if (lastCompensation != null)
                {
                    var lastNumber = int.Parse(lastCompensation.CompensationId.Replace("COMP-", ""));
                    newId = lastNumber + 1;
                }
                var compensationId = $"COMP-{newId:D3}";

                // Calculate net salary
                var netSalary = viewModel.BaseSalary + viewModel.Bonus - viewModel.Deductions;

                var compensation = new Compensation
                {
                    CompensationId = compensationId,
                    EmployeeId = viewModel.EmployeeId,
                    EmployeeName = viewModel.EmployeeName,
                    PhoneNumber = viewModel.PhoneNumber,
                    Position = viewModel.Position,
                    Department = viewModel.Department,
                    BaseSalary = viewModel.BaseSalary,
                    Bonus = viewModel.Bonus,
                    Deductions = viewModel.Deductions,
                    NetSalary = netSalary,
                    PaymentDate = viewModel.PaymentDate,
                    PaymentMethod = viewModel.PaymentMethod,
                    PaymentStatus = viewModel.Status ?? "Pending",
                    Notes = viewModel.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Compensations.Add(compensation);
                await _context.SaveChangesAsync();

                // ===== CREATE NOTIFICATION FOR NEW COMPENSATION RECORD =====
                var period = $"{compensation.PaymentDate:MMMM yyyy}";
                await _notificationService.CreateCompensationCreatedNotification(
                    employeeName: compensation.EmployeeName,
                    amount: compensation.NetSalary,
                    period: period,
                    actionUrl: $"/Compensation/Details/{compensation.Id}"
                );

                // ===== CHECK IF APPROVAL REQUIRED (if amount > threshold) =====
                if (compensation.NetSalary > 10000) // Example threshold for approval
                {
                    await _notificationService.CreateNotificationAsync(
                        title: "Compensation Approval Required",
                        description: $"Compensation for {compensation.EmployeeName} (${compensation.NetSalary:N2}) requires your approval",
                        module: "Compensation",
                        type: NotificationType.Warning,
                        actionUrl: $"/Compensation/Edit/{compensation.Id}",
                        actionText: "Review Now"
                    );
                }
                // ============================================================

                TempData["Success"] = $"Compensation {compensationId} created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }
        // =========================
        // REMOTE VALIDATION — Check if Phone Number already exists
        // Called by [Remote] attribute on PhoneNumber field as user types
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckPhoneNumber(string phoneNumber)
        {
            var existingPhone = await _context.Compensations
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (existingPhone != null)
            {
                // Return error message string — jQuery Validate treats any string as an error
                return Json($"Phone number {phoneNumber} already exists.");
            }

            // Return true — jQuery Validate treats true as valid
            return Json(true);
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var compensation = await _context.Compensations.FindAsync(id);
            if (compensation == null)
                return NotFound();

            var viewModel = new CompensationEditViewModel
            {
                Id = compensation.Id,
                EmployeeId = compensation.EmployeeId,
                EmployeeName = compensation.EmployeeName,
                PhoneNumber = compensation.PhoneNumber,
                Position = compensation.Position,
                Department = compensation.Department,
                BaseSalary = compensation.BaseSalary,
                Bonus = compensation.Bonus,
                Deductions = compensation.Deductions,
                PaymentDate = compensation.PaymentDate,
                PaymentMethod = compensation.PaymentMethod,
                Status = compensation.PaymentStatus,
                Notes = compensation.Notes
            };

            return View(viewModel);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompensationEditViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var compensation = await _context.Compensations.FindAsync(id);
                    if (compensation == null)
                        return NotFound();

                    // Check if Employee ID is being changed and if it already exists
                    if (compensation.EmployeeId != viewModel.EmployeeId)
                    {
                        var existingEmployee = await _context.Compensations
                            .FirstOrDefaultAsync(c => c.EmployeeId == viewModel.EmployeeId && c.Id != id);

                        if (existingEmployee != null)
                        {
                            ModelState.AddModelError("EmployeeId", "Employee ID already exists");
                            return View(viewModel);
                        }
                    }

                    // Store old payment status for comparison
                    var oldPaymentStatus = compensation.PaymentStatus;

                    // Calculate net salary
                    var netSalary = viewModel.BaseSalary + viewModel.Bonus - viewModel.Deductions;

                    // Update properties
                    compensation.EmployeeId = viewModel.EmployeeId;
                    compensation.EmployeeName = viewModel.EmployeeName;
                    compensation.Position = viewModel.Position;
                    compensation.Department = viewModel.Department;
                    compensation.BaseSalary = viewModel.BaseSalary;
                    compensation.Bonus = viewModel.Bonus;
                    compensation.Deductions = viewModel.Deductions;
                    compensation.NetSalary = netSalary;
                    compensation.PaymentDate = viewModel.PaymentDate;
                    compensation.PaymentMethod = viewModel.PaymentMethod;
                    compensation.PaymentStatus = viewModel.Status;
                    compensation.Notes = viewModel.Notes;
                    compensation.UpdatedAt = DateTime.UtcNow;

                    // If status changed to Paid, set ProcessedAt
                    if (viewModel.Status == "Paid" && oldPaymentStatus != "Paid")
                    {
                        compensation.ProcessedAt = DateTime.UtcNow;

                        // ===== CREATE NOTIFICATION FOR COMPENSATION PAID =====
                        await _notificationService.CreateCompensationPaidNotification(
                            employeeName: compensation.EmployeeName,
                            amount: compensation.NetSalary,
                            actionUrl: $"/Compensation/Details/{compensation.Id}"
                        );
                    }
                    // ========================================================

                    // If status changed to Pending (needs approval)
                    if (viewModel.Status == "Pending" && oldPaymentStatus != "Pending")
                    {
                        await _notificationService.CreateNotificationAsync(
                            title: "Compensation Pending Approval",
                            description: $"Compensation for {compensation.EmployeeName} (${compensation.NetSalary:N2}) is pending approval",
                            module: "Compensation",
                            type: NotificationType.Info,
                            actionUrl: $"/Compensation/Edit/{compensation.Id}",
                            actionText: "Review"
                        );
                    }

                    await _context.SaveChangesAsync();

                    // ===== CREATE NOTIFICATION FOR COMPENSATION UPDATED =====
                    if (oldPaymentStatus == viewModel.Status)
                    {
                        await _notificationService.CreateNotificationAsync(
                            title: $"Compensation Updated",
                            description: $"Compensation record for {compensation.EmployeeName} has been updated",
                            module: "Compensation",
                            type: NotificationType.Info,
                            actionUrl: $"/Compensation/Details/{compensation.Id}",
                            actionText: "View Record"
                        );
                    }
                    // ========================================================

                    TempData["Success"] = $"Compensation updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CompensationExists(viewModel.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(viewModel);
        }

        // =========================
        // DETAILS (GET)
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var compensation = await _context.Compensations.FindAsync(id);
            if (compensation == null)
                return NotFound();

            var viewModel = new CompensationListItemViewModel
            {
                Id = compensation.Id,
                EmployeeId = compensation.EmployeeId,
                EmployeeName = compensation.EmployeeName,
                PhoneNumber = compensation.PhoneNumber,
                Position = compensation.Position,
                Department = compensation.Department,
                BaseSalary = compensation.BaseSalary,
                Bonus = compensation.Bonus,
                Deductions = compensation.Deductions,
                NetSalary = compensation.NetSalary,
                PaymentDate = compensation.PaymentDate,
                PaymentMethod = compensation.PaymentMethod,
                Status = compensation.PaymentStatus,
                Notes = compensation.Notes
            };

            return View(viewModel);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var compensation = await _context.Compensations.FindAsync(id);
                if (compensation != null)
                {
                    var employeeName = compensation.EmployeeName;

                    _context.Compensations.Remove(compensation);
                    await _context.SaveChangesAsync();

                    // ===== CREATE NOTIFICATION FOR DELETED COMPENSATION =====
                    await _notificationService.CreateNotificationAsync(
                        title: $"Compensation Deleted",
                        description: $"Compensation record for {employeeName} has been deleted",
                        module: "Compensation",
                        type: NotificationType.Warning,
                        actionUrl: "/Compensation",
                        actionText: "View Compensation"
                    );
                    // ========================================================

                    TempData["Success"] = $"Compensation deleted successfully.";
                }
                else
                {
                    TempData["Error"] = $"Compensation with ID {id} not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting compensation: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // REMOTE VALIDATION - Check if Employee ID exists
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckEmployeeId(string employeeId)
        {
            var existingEmployee = await _context.Compensations
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId);

            if (existingEmployee != null)
            {
                return Json($"Employee ID {employeeId} already exists.");
            }

            return Json(true);
        }

        // =========================
        // HELPER METHODS
        // =========================
        private async Task<bool> CompensationExists(int id)
        {
            return await _context.Compensations.AnyAsync(c => c.Id == id);
        }
    }
}