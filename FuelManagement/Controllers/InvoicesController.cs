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

namespace FuelManagement.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private const decimal VAT_RATE = 0.05m; // 5% Somali VAT
        private const int PageSize = 10;

        public InvoicesController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }


        // =========================
        // INDEX - List all invoices with filters and pagination
        // =========================
        public async Task<IActionResult> Index(InvoiceFilterViewModel filter, int page = 1)
        {
            // Initialize filter if null
            if (filter == null)
                filter = new InvoiceFilterViewModel();

            // Get available filter options from database
            var dbCustomerTypes = await _context.Invoices
                .Select(i => i.CustomerType)
                .Where(ct => !string.IsNullOrEmpty(ct))
                .Distinct()
                .OrderBy(ct => ct)
                .ToListAsync();

            var dbPaymentStatuses = await _context.Invoices
                .Select(i => i.PaymentStatus)
                .Where(ps => !string.IsNullOrEmpty(ps))
                .Distinct()
                .OrderBy(ps => ps)
                .ToListAsync();

            var dbFuelTypes = await _context.Invoices
                .Select(i => i.FuelType)
                .Where(ft => !string.IsNullOrEmpty(ft))
                .Distinct()
                .OrderBy(ft => ft)
                .ToListAsync();

            // Combine database values with default values to ensure all options appear
            filter.AvailableCustomerTypes = InvoiceFilterViewModel.DefaultCustomerTypes
                .Union(dbCustomerTypes)
                .Distinct()
                .OrderBy(ct => ct)
                .ToList();

            filter.AvailablePaymentStatuses = InvoiceFilterViewModel.DefaultPaymentStatuses
                .Union(dbPaymentStatuses)
                .Distinct()
                .OrderBy(ps => ps)
                .ToList();

            filter.AvailableFuelTypes = InvoiceFilterViewModel.DefaultFuelTypes
                .Union(dbFuelTypes)
                .Distinct()
                .OrderBy(ft => ft)
                .ToList();

            // Build query with filters
            var query = _context.Invoices.AsQueryable();

            // Apply date filters
            if (filter.StartDate.HasValue)
                query = query.Where(i => i.IssueDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(i => i.IssueDate <= filter.EndDate.Value);

            // Apply text filters
            if (!string.IsNullOrEmpty(filter.CustomerName))
                query = query.Where(i => i.CustomerName.Contains(filter.CustomerName) ||
                                         i.InvoiceNumber.Contains(filter.CustomerName));

            if (!string.IsNullOrEmpty(filter.CustomerType))
                query = query.Where(i => i.CustomerType == filter.CustomerType);

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                query = query.Where(i => i.PaymentStatus == filter.PaymentStatus);

            if (!string.IsNullOrEmpty(filter.FuelType))
                query = query.Where(i => i.FuelType == filter.FuelType);

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // FIXED: Properly order by invoice number numerically
            // Extract the numeric part and order by that
            var invoices = await query
                .Select(i => new { Invoice = i, SortOrder = i.InvoiceNumber }) // We'll sort in memory for complex ordering
                .ToListAsync();

            // Custom sorting: extract numbers from invoice numbers and sort numerically
            var sortedInvoices = invoices
                .Select(x => x.Invoice)
                .OrderBy(i => {
                    // Try to extract numeric part from invoice number
                    // Assuming format like "inv-001" or "INV-2024-001"
                    var match = System.Text.RegularExpressions.Regex.Match(i.InvoiceNumber, @"\d+$");
                    if (match.Success && int.TryParse(match.Value, out int num))
                    {
                        return num;
                    }
                    // Fallback to string comparison
                    return 0;
                })
                .ThenBy(i => i.InvoiceNumber) // Secondary sort by string
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Calculate summary data from ALL filtered invoices
            var allFilteredList = await query.ToListAsync();

            var viewModel = new InvoicesViewModel
            {
                Filter = filter,
                InvoiceItems = sortedInvoices.Select(i => new InvoiceListItemViewModel
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    CustomerName = i.CustomerName,
                    CustomerPhone = i.CustomerPhone,
                    CustomerType = i.CustomerType,
                    FuelType = i.FuelType,
                    TotalLiters = i.TotalLiters,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal,
                    VAT = i.VAT,
                    TotalAmount = i.TotalAmount,
                    AmountPaid = i.AmountPaid,
                    BalanceDue = i.BalanceDue,
                    PaymentStatus = i.PaymentStatus,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    CreatedBy = i.CreatedBy ?? "System",
                    CreatedAt = i.CreatedAt
                }).ToList(),

                // Total number of invoices
                TotalInvoices = allFilteredList.Count,

                // Summary data from ALL filtered invoices (for cards)
                TotalRevenue = allFilteredList.Sum(i => i.TotalAmount), // This is correct - reads from Total column

                // FIXED: TotalPaid should sum AmountPaid (actual money received), not TotalAmount
                TotalPaid = allFilteredList.Where(i => i.PaymentStatus == "Paid").Sum(i => i.AmountPaid),

                TotalOutstanding = allFilteredList.Sum(i => i.BalanceDue),
                TotalOverdue = allFilteredList.Where(i => i.PaymentStatus == "Overdue").Sum(i => i.BalanceDue),
                TotalPartial = allFilteredList.Where(i => i.PaymentStatus == "Partial").Sum(i => i.BalanceDue),

                // Count statistics
                PaidCount = allFilteredList.Count(i => i.PaymentStatus == "Paid"),
                UnpaidCount = allFilteredList.Count(i => i.PaymentStatus == "Unpaid"),
                PartialCount = allFilteredList.Count(i => i.PaymentStatus == "Partial"),
                OverdueCount = allFilteredList.Count(i => i.PaymentStatus == "Overdue"),

                // VAT total
                TotalVAT = allFilteredList.Sum(i => i.VAT),

                // Calculate additional metrics
                AverageInvoiceValue = allFilteredList.Any() ? allFilteredList.Average(i => i.TotalAmount) : 0,

                // FIXED: Collection rate should use AmountPaid vs TotalAmount
                CollectionRate = allFilteredList.Any() && allFilteredList.Sum(i => i.TotalAmount) > 0
                    ? (allFilteredList.Sum(i => i.AmountPaid) / allFilteredList.Sum(i => i.TotalAmount)) * 100
                    : 0
            };

            // Store pagination info in ViewBag
            ViewBag.PageNumber = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / PageSize);
            ViewBag.CurrentFilter = filter;

            return View(viewModel);
        }
        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            var viewModel = new InvoiceCreateViewModel
            {
                IssueDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30)
            };
            return View(viewModel);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if Invoice Number already exists
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.InvoiceNumber == viewModel.InvoiceNumber);

                if (existingInvoice != null)
                {
                    ModelState.AddModelError("InvoiceNumber", "Invoice Number already exists. Please use a different number.");
                    return View(viewModel);
                }

                // Check if Phone Number already exists
                if (!string.IsNullOrEmpty(viewModel.CustomerPhone))
                {
                    var existingPhone = await _context.Invoices
                        .FirstOrDefaultAsync(i => i.CustomerPhone == viewModel.CustomerPhone);

                    if (existingPhone != null)
                    {
                        ModelState.AddModelError("CustomerPhone", "This Phone Number already exists");
                        return View(viewModel);
                    }
                }

                // Calculate derived values
                var subtotal = viewModel.TotalLiters * viewModel.UnitPrice;
                var vat = subtotal * VAT_RATE;
                var totalAmount = subtotal + vat;
                var balanceDue = totalAmount - viewModel.AmountPaid;

                var paymentStatus = viewModel.PaymentStatus;

                var invoice = new Invoice
                {
                    InvoiceNumber = viewModel.InvoiceNumber,
                    CustomerName = viewModel.CustomerName,
                    CustomerType = viewModel.CustomerType,
                    CustomerPhone = viewModel.CustomerPhone,
                    CustomerEmail = viewModel.CustomerEmail,
                    CustomerAddress = viewModel.CustomerAddress,
                    FuelType = viewModel.FuelType,
                    TotalLiters = viewModel.TotalLiters,
                    UnitPrice = viewModel.UnitPrice,
                    Subtotal = subtotal,
                    VAT = vat,
                    TotalAmount = totalAmount,
                    AmountPaid = viewModel.AmountPaid,
                    BalanceDue = balanceDue,
                    PaymentStatus = paymentStatus,
                    IssueDate = viewModel.IssueDate,
                    DueDate = viewModel.DueDate,
                    CreatedBy = viewModel.CreatedBy ?? "Admin",
                    CreatedAt = DateTime.UtcNow,
                    Notes = viewModel.Notes
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // ===== CREATE NOTIFICATION FOR NEW INVOICE =====
                await _notificationService.CreateInvoiceCreatedNotification(
                    invoiceNumber: invoice.InvoiceNumber,
                    customerName: invoice.CustomerName,
                    amount: invoice.TotalAmount,
                    actionUrl: $"/Invoices/Details/{invoice.InvoiceNumber}"
                );
                // ===============================================

                TempData["Success"] = $"Invoice {viewModel.InvoiceNumber} created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // =========================
        // EDIT (GET) - Using InvoiceNumber instead of Id
        // =========================
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceNumber == id);

            if (invoice == null)
                return NotFound();

            var viewModel = new InvoiceEditViewModel
            {
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.CustomerName,
                CustomerType = invoice.CustomerType,
                CustomerPhone = invoice.CustomerPhone,
                CustomerEmail = invoice.CustomerEmail,
                CustomerAddress = invoice.CustomerAddress,
                FuelType = invoice.FuelType,
                TotalLiters = invoice.TotalLiters,
                UnitPrice = invoice.UnitPrice,
                AmountPaid = invoice.AmountPaid,
                PaymentStatus = invoice.PaymentStatus,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Notes = invoice.Notes,
                CreatedBy = invoice.CreatedBy
            };

            return View(viewModel);
        }

        // =========================
        // EDIT (POST) - Using InvoiceNumber instead of Id
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, InvoiceEditViewModel viewModel)
        {
            if (id != viewModel.InvoiceNumber)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var invoice = await _context.Invoices
                        .FirstOrDefaultAsync(i => i.InvoiceNumber == id);

                    if (invoice == null)
                        return NotFound();

                    // Check if Phone Number already exists (excluding current invoice)
                    if (!string.IsNullOrEmpty(viewModel.CustomerPhone))
                    {
                        var existingPhone = await _context.Invoices
                            .FirstOrDefaultAsync(i => i.CustomerPhone == viewModel.CustomerPhone
                                && i.InvoiceNumber != viewModel.InvoiceNumber);

                        if (existingPhone != null)
                        {
                            ModelState.AddModelError("CustomerPhone", "This Phone Number already exists");
                            return View(viewModel);
                        }
                    }

                    // Store old payment status for comparison
                    var oldPaymentStatus = invoice.PaymentStatus;

                    // Calculate derived values
                    var subtotal = viewModel.TotalLiters * viewModel.UnitPrice;
                    var vat = subtotal * VAT_RATE;
                    var totalAmount = subtotal + vat;
                    var balanceDue = totalAmount - viewModel.AmountPaid;

                    var paymentStatus = viewModel.PaymentStatus;

                    // Update properties
                    invoice.CustomerName = viewModel.CustomerName;
                    invoice.CustomerType = viewModel.CustomerType;
                    invoice.CustomerPhone = viewModel.CustomerPhone;
                    invoice.CustomerEmail = viewModel.CustomerEmail;
                    invoice.CustomerAddress = viewModel.CustomerAddress;
                    invoice.FuelType = viewModel.FuelType;
                    invoice.TotalLiters = viewModel.TotalLiters;
                    invoice.UnitPrice = viewModel.UnitPrice;
                    invoice.Subtotal = subtotal;
                    invoice.VAT = vat;
                    invoice.TotalAmount = totalAmount;
                    invoice.AmountPaid = viewModel.AmountPaid;
                    invoice.BalanceDue = balanceDue;
                    invoice.PaymentStatus = paymentStatus;
                    invoice.IssueDate = viewModel.IssueDate;
                    invoice.DueDate = viewModel.DueDate;
                    invoice.Notes = viewModel.Notes;
                    invoice.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // ===== CREATE NOTIFICATION FOR INVOICE UPDATED (GENERAL) =====
                    if (oldPaymentStatus == paymentStatus)
                    {
                        await _notificationService.CreateNotificationAsync(
                            title: $"Invoice Updated: {invoice.InvoiceNumber}",
                            description: $"Invoice for {invoice.CustomerName} has been updated",
                            module: "Invoices",
                            type: NotificationType.Info,
                            actionUrl: Url.Action("Details", "Invoices", new { id = invoice.InvoiceNumber }),
                            actionText: "View Invoice"
                        );
                    }
                    // ============================================================

                    // ===== CHECK IF INVOICE WAS MARKED AS PAID =====
                    if (oldPaymentStatus != "Paid" && paymentStatus == "Paid")
                    {
                        await _notificationService.CreateInvoicePaidNotification(
                            invoiceNumber: invoice.InvoiceNumber,
                            customerName: invoice.CustomerName,
                            amount: invoice.TotalAmount,
                           actionUrl: Url.Action("Details", "Invoices", new { id = invoice.InvoiceNumber })
                        );
                    }
                    // ================================================

                    TempData["Success"] = $"Invoice {invoice.InvoiceNumber} updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await InvoiceExists(viewModel.InvoiceNumber))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(viewModel);
        }

        // =========================
        // DETAILS (GET) - Using InvoiceNumber instead of Id
        // =========================
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceNumber == id);

            if (invoice == null)
                return NotFound();

            // ===== CHECK IF INVOICE IS OVERDUE =====
            if (invoice.PaymentStatus != "Paid" && invoice.DueDate < DateTime.UtcNow.Date)
            {
                var daysOverdue = (DateTime.UtcNow.Date - invoice.DueDate).Days;

                // You might want to trigger this from a background job instead of on every page view
                // But for now, we can check if it's newly overdue
                if (invoice.PaymentStatus != "Overdue")
                {
                    // This would be better in a scheduled job, but for demo purposes:
                    // await _notificationService.CreateOverdueInvoiceNotification(...);
                }
            }
            // =======================================

            // Get related receipts for this invoice
            var relatedReceipts = await _context.Receipts
                .Where(r => r.InvoiceId == invoice.Id || r.InvoiceRef == invoice.InvoiceNumber)
                .OrderByDescending(r => r.PaymentDate)
                .Select(r => new ReceiptListItemViewModel
                {
                    Id = r.Id,
                    ReceiptNo = r.ReceiptNo,
                    InvoiceRef = r.InvoiceRef,
                    CustomerName = r.CustomerName,
                    CustomerType = r.CustomerType,
                    FuelType = r.FuelType,
                    Quantity = r.Quantity,
                    UnitPrice = r.UnitPrice,
                    Subtotal = r.Subtotal,
                    VAT = r.VAT,
                    TotalAmount = r.TotalAmount,
                    PaymentMethod = r.PaymentMethod,
                    PaymentStatus = r.PaymentStatus,
                    PaymentDate = r.PaymentDate,
                    CreatedBy = r.CreatedBy,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var viewModel = new InvoiceDetailsViewModel
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.CustomerName,
                CustomerType = invoice.CustomerType,
                CustomerAddress = invoice.CustomerAddress,
                CustomerPhone = invoice.CustomerPhone,
                CustomerEmail = invoice.CustomerEmail,
                FuelType = invoice.FuelType,
                TotalLiters = invoice.TotalLiters,
                UnitPrice = invoice.UnitPrice,
                Subtotal = invoice.Subtotal,
                VAT = invoice.VAT,
                TotalAmount = invoice.TotalAmount,
                AmountPaid = invoice.AmountPaid,
                BalanceDue = invoice.BalanceDue,
                PaymentStatus = invoice.PaymentStatus,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                CreatedBy = invoice.CreatedBy,
                CreatedAt = invoice.CreatedAt,
                Notes = invoice.Notes,
                // Company Info - could come from settings table later
                CompanyName = "Uniso Fuel Inc.",
                CompanyAddress = "KM5, Mogadishu, Somalia",
                CompanyPhone = "+252 61 249 6070",
                CompanyEmail = "ayuuboodaaye2@gmail.com",
                // Add related receipts
                RelatedReceipts = relatedReceipts
            };

            return View(viewModel);
        }

        // =========================
        // DELETE (POST) - Using InvoiceNumber instead of Id
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "Invoice number is required.";
                    return RedirectToAction(nameof(Index));
                }

                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.InvoiceNumber == id);

                if (invoice != null)
                {
                    var invoiceInfo = $"{invoice.InvoiceNumber} - {invoice.CustomerName}";

                    _context.Invoices.Remove(invoice);
                    await _context.SaveChangesAsync();

                    // ===== CREATE NOTIFICATION FOR DELETED INVOICE =====
                    await _notificationService.CreateNotificationAsync(
                        title: $"Invoice Deleted: {invoice.InvoiceNumber}",
                        description: $"Invoice for {invoice.CustomerName} has been deleted",
                        module: "Invoices",
                        type: NotificationType.Warning,
                        actionUrl: "/Invoices",
                        actionText: "View Invoices"
                    );
                    // ====================================================

                    TempData["Success"] = $"Invoice {invoice.InvoiceNumber} deleted successfully.";
                }
                else
                {
                    TempData["Error"] = $"Invoice with number {id} not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting invoice: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // REMOTE VALIDATION - Check if Invoice Number exists
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckInvoiceNumber(string invoiceNumber)
        {
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

            if (existingInvoice != null)
            {
                return Json($"Invoice Number {invoiceNumber} already exists.");
            }

            return Json(true);
        }

        // =========================
        // REMOTE VALIDATION - Check if Phone Number exists (for CREATE)
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckCustomerPhone(string customerPhone)
        {
            if (string.IsNullOrEmpty(customerPhone))
                return Json(true);

            var existingPhone = await _context.Invoices
                .FirstOrDefaultAsync(i => i.CustomerPhone == customerPhone);

            if (existingPhone != null)
            {
                return Json("This Phone Number already exists");
            }

            return Json(true);
        }

        // =========================
        // REMOTE VALIDATION - Check if Phone Number exists (for EDIT)
        // =========================
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckCustomerPhoneForEdit(string customerPhone, string invoiceNumber)
        {
            if (string.IsNullOrEmpty(customerPhone))
                return Json(true);

            var existingPhone = await _context.Invoices
                .FirstOrDefaultAsync(i => i.CustomerPhone == customerPhone
                    && i.InvoiceNumber != invoiceNumber);

            if (existingPhone != null)
            {
                return Json("This Phone Number already exists");
            }

            return Json(true);
        }

        // =========================
        // HELPER METHODS
        // =========================
        private string DeterminePaymentStatus(decimal amountPaid, decimal totalAmount)
        {
            if (amountPaid >= totalAmount)
                return "Paid";
            if (amountPaid == 0)
                return "Unpaid";
            if (amountPaid > 0 && amountPaid < totalAmount)
                return "Partial";
            return "Unpaid";
        }

        private async Task<bool> InvoiceExists(string invoiceNumber)
        {
            return await _context.Invoices.AnyAsync(i => i.InvoiceNumber == invoiceNumber);
        }
    }
}