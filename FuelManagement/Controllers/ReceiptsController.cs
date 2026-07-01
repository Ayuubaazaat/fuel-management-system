using Microsoft.AspNetCore.Mvc;
using FuelManagement.Models;
using FuelManagement.Data;
using FuelManagement.Services.Interfaces;
using FuelManagement.Models.Enums;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FuelManagement.Controllers
{
    public class ReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private const decimal VAT_RATE = 0.05m; // 5% Somali VAT
        private const int PageSize = 10; // Show 10 rows per page

        public ReceiptsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Receipts
        public async Task<IActionResult> Index(ReceiptFilterViewModel filter, int page = 1)
        {
            // Start with base query
            var query = _context.Receipts.AsQueryable();

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(r => r.PaymentDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.PaymentDate <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.CustomerType))
                query = query.Where(r => r.CustomerType == filter.CustomerType);

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                query = query.Where(r => r.PaymentStatus == filter.PaymentStatus);

            if (!string.IsNullOrEmpty(filter.PaymentMethod))
                query = query.Where(r => r.PaymentMethod == filter.PaymentMethod);

            if (!string.IsNullOrEmpty(filter.FuelType))
                query = query.Where(r => r.FuelType == filter.FuelType);

            if (!string.IsNullOrEmpty(filter.CustomerName))
                query = query.Where(r => r.CustomerName.Contains(filter.CustomerName) ||
                                         r.ReceiptNo.Contains(filter.CustomerName) ||
                                         r.InvoiceRef.Contains(filter.CustomerName));

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Get all receipts first for proper sorting by invoice reference
            var allReceipts = await query.ToListAsync();

            // Calculate previous month's data for percentage change
            var firstDayOfThisMonth = new DateTime(DateTime.UtcNow.Date.Year, DateTime.UtcNow.Date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
            var lastDayOfLastMonth = firstDayOfThisMonth.AddDays(-1);

            var lastMonthReceipts = await _context.Receipts
                .Where(r => r.PaymentDate >= firstDayOfLastMonth && r.PaymentDate <= lastDayOfLastMonth)
                .ToListAsync();

            var previousMonthTotalReceipts = lastMonthReceipts.Count;
            var previousMonthTotalCollected = lastMonthReceipts
                .Where(r => r.PaymentStatus == "Paid" || r.PaymentStatus == "Partial")
                .Sum(r => r.AmountPaid ?? r.TotalAmount);

            // Sort by Invoice Reference numerically (INV-001, INV-002, INV-003, etc.)
            var sortedReceipts = allReceipts
                .OrderBy(r => {
                    // Extract numeric part from Invoice Reference
                    var match = Regex.Match(r.InvoiceRef ?? "", @"\d+");
                    if (match.Success && int.TryParse(match.Value, out int num))
                    {
                        return num;
                    }
                    return 0;
                })
                .ThenBy(r => r.InvoiceRef) // Secondary sort by string
                .ToList();

            // Apply pagination
            var pagedReceipts = sortedReceipts
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Map to ListItem ViewModel
            var receiptItems = pagedReceipts.Select(r => new ReceiptListItemViewModel
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
            }).ToList();

            // Calculate summary statistics from ALL filtered receipts (not just current page)
            var totalAmount = allReceipts.Sum(r => r.TotalAmount);
            var collectedAmount = allReceipts.Where(r => r.PaymentStatus == "Paid" || r.PaymentStatus == "Partial")
                                         .Sum(r => r.AmountPaid ?? r.TotalAmount);
            var pendingAmount = allReceipts.Where(r => r.PaymentStatus == "Pending").Sum(r => r.TotalAmount);
            var partialAmount = allReceipts.Where(r => r.PaymentStatus == "Partial").Sum(r => r.TotalAmount - (r.AmountPaid ?? 0));
            var overdueAmount = allReceipts.Where(r => r.PaymentStatus == "Overdue").Sum(r => r.TotalAmount);

            var collectionRate = totalAmount > 0
                ? Math.Round((collectedAmount / totalAmount) * 100, 2)
                : 0;

            var viewModel = new ReceiptsViewModel
            {
                Filter = filter ?? new ReceiptFilterViewModel
                {
                    StartDate = DateTime.UtcNow.Date.AddDays(-30),
                    EndDate = DateTime.UtcNow.Date
                },
                ReceiptItems = receiptItems,
                TotalReceipts = allReceipts.Count,
                TotalAmountCollected = collectedAmount,
                CollectionRate = collectionRate,
                PaidCount = allReceipts.Count(r => r.PaymentStatus == "Paid"),
                PendingCount = allReceipts.Count(r => r.PaymentStatus == "Pending"),
                PendingAmount = pendingAmount,
                PartialCount = allReceipts.Count(r => r.PaymentStatus == "Partial"),
                PartialAmount = partialAmount,
                OverdueCount = allReceipts.Count(r => r.PaymentStatus == "Overdue"),
                OverdueAmount = overdueAmount,
                VoidCount = allReceipts.Count(r => r.PaymentStatus == "Void"),
                PreviousMonthTotalReceipts = previousMonthTotalReceipts,
                PreviousMonthTotalCollected = previousMonthTotalCollected
            };

            // Store pagination info in ViewBag for the view
            ViewBag.PageNumber = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / PageSize);
            ViewBag.CurrentFilter = filter;

            return View(viewModel);
        }

        // GET: Receipts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _context.Receipts
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null)
            {
                return NotFound();
            }

            var viewModel = new ReceiptDetailsViewModel
            {
                Id = receipt.Id,
                ReceiptNo = receipt.ReceiptNo,
                InvoiceRef = receipt.InvoiceRef,
                CustomerName = receipt.CustomerName,
                CustomerType = receipt.CustomerType,
                CustomerPhone = receipt.CustomerPhone,
                CustomerEmail = receipt.CustomerEmail,
                FuelType = receipt.FuelType,
                Quantity = receipt.Quantity,
                UnitPrice = receipt.UnitPrice,
                Subtotal = receipt.Subtotal,
                VAT = receipt.VAT,
                TotalAmount = receipt.TotalAmount,
                AmountPaid = receipt.AmountPaid ?? 0,
                BalanceRemaining = receipt.TotalAmount - (receipt.AmountPaid ?? 0),
                PaymentMethod = receipt.PaymentMethod,
                PaymentStatus = receipt.PaymentStatus,
                PaymentDate = receipt.PaymentDate,
                CreatedBy = receipt.CreatedBy,
                CreatedAt = receipt.CreatedAt,
                StationName = "Uniso Fuel",
                StationAddress = "KM4, Mogadishu, Somalia",
                StationPhone = "+252 61 249 6070",
                StationEmail = "ayuubboodaaye@gmail.com",
                Notes = receipt.Notes,
                InvoiceId = receipt.InvoiceId
            };

            return View(viewModel);
        }

        // GET: Receipts/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ReceiptCreateViewModel
            {
                PaymentDate = DateTime.UtcNow.Date,
                CustomerType = "Walk-in",
                PaymentStatus = "Pending",
                Invoices = await GetInvoicesSelectList()
            };

            return View(viewModel);
        }

        // POST: Receipts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiptCreateViewModel viewModel)
        {
            // If an invoice was selected, load its details
            if (viewModel.SelectedInvoiceId.HasValue && viewModel.SelectedInvoiceId.Value > 0)
            {
                var invoice = await _context.Invoices.FindAsync(viewModel.SelectedInvoiceId.Value);
                if (invoice != null)
                {
                    viewModel.InvoiceRef = invoice.InvoiceNumber;
                    viewModel.CustomerName = invoice.CustomerName;
                    viewModel.CustomerType = invoice.CustomerType;
                    viewModel.CustomerPhone = invoice.CustomerPhone;
                    viewModel.CustomerEmail = invoice.CustomerEmail;
                    viewModel.FuelType = invoice.FuelType;
                    viewModel.Quantity = invoice.TotalLiters;
                    viewModel.UnitPrice = invoice.UnitPrice;
                    viewModel.InvoiceId = invoice.Id;
                }
            }

            if (ModelState.IsValid)
            {
                // Check if Invoice Reference already exists
                var existingInvoiceRef = await _context.Receipts
                    .FirstOrDefaultAsync(r => r.InvoiceRef == viewModel.InvoiceRef);

                if (existingInvoiceRef != null)
                {
                    ModelState.AddModelError("InvoiceRef", "Invoice reference already exists.");
                    viewModel.Invoices = await GetInvoicesSelectList();
                    return View(viewModel);
                }

                // Check if Phone Number already exists
                if (!string.IsNullOrEmpty(viewModel.CustomerPhone))
                {
                    var existingPhone = await _context.Receipts
                        .FirstOrDefaultAsync(r => r.CustomerPhone == viewModel.CustomerPhone);

                    if (existingPhone != null)
                    {
                        ModelState.AddModelError("CustomerPhone", "Phone number already exists.");
                        viewModel.Invoices = await GetInvoicesSelectList();
                        return View(viewModel);
                    }
                }

                // Calculate amounts
                var subtotal = viewModel.Quantity * viewModel.UnitPrice;
                var vat = subtotal * VAT_RATE;
                var totalAmount = subtotal + vat;

                // Generate receipt number
                var receiptNo = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

                var receipt = new Receipt
                {
                    ReceiptNo = receiptNo,
                    InvoiceRef = viewModel.InvoiceRef,
                    InvoiceId = viewModel.InvoiceId,
                    CustomerName = viewModel.CustomerName,
                    CustomerType = viewModel.CustomerType,
                    CustomerPhone = viewModel.CustomerPhone,
                    CustomerEmail = viewModel.CustomerEmail,
                    FuelType = viewModel.FuelType,
                    Quantity = viewModel.Quantity,
                    UnitPrice = viewModel.UnitPrice,
                    Subtotal = subtotal,
                    VAT = vat,
                    TotalAmount = totalAmount,
                    AmountPaid = viewModel.AmountPaid,
                    PaymentMethod = viewModel.PaymentMethod,
                    PaymentStatus = viewModel.PaymentStatus,
                    PaymentDate = viewModel.PaymentDate,
                    Notes = viewModel.Notes,
                    CreatedBy = User.Identity.Name ?? "System",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();

                // ===== NOTIFICATION: New Receipt Generated =====
                await _notificationService.CreateReceiptGeneratedNotification(
                    receiptNo: receipt.ReceiptNo,
                    customerName: receipt.CustomerName,
                    amount: receipt.TotalAmount,
                    actionUrl: $"/Receipts/Details/{receipt.Id}"
                );
                // ===============================================

                TempData["Success"] = "Receipt created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed - redisplay form
            viewModel.Invoices = await GetInvoicesSelectList();
            return View(viewModel);
        }

        // GET: Receipts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var receipt = await _context.Receipts.FindAsync(id);
            if (receipt == null)
            {
                return NotFound();
            }

            var viewModel = new ReceiptEditViewModel
            {
                Id = receipt.Id,
                ReceiptNo = receipt.ReceiptNo,
                InvoiceRef = receipt.InvoiceRef,
                InvoiceId = receipt.InvoiceId,
                CustomerName = receipt.CustomerName,
                CustomerType = receipt.CustomerType,
                CustomerPhone = receipt.CustomerPhone,
                CustomerEmail = receipt.CustomerEmail,
                FuelType = receipt.FuelType,
                Quantity = receipt.Quantity,
                UnitPrice = receipt.UnitPrice,
                PaymentMethod = receipt.PaymentMethod,
                PaymentStatus = receipt.PaymentStatus,
                AmountPaid = receipt.AmountPaid,
                PaymentDate = receipt.PaymentDate,
                Notes = receipt.Notes,
                Invoices = await GetInvoicesSelectList()
            };

            // Set the selected invoice if exists
            if (receipt.InvoiceId.HasValue)
            {
                viewModel.SelectedInvoiceId = receipt.InvoiceId.Value;
            }

            return View(viewModel);
        }

        // POST: Receipts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReceiptEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            // If an invoice was selected, load its details
            if (viewModel.SelectedInvoiceId.HasValue && viewModel.SelectedInvoiceId.Value > 0)
            {
                var invoice = await _context.Invoices.FindAsync(viewModel.SelectedInvoiceId.Value);
                if (invoice != null)
                {
                    viewModel.InvoiceRef = invoice.InvoiceNumber;
                    viewModel.CustomerName = invoice.CustomerName;
                    viewModel.CustomerType = invoice.CustomerType;
                    viewModel.CustomerPhone = invoice.CustomerPhone;
                    viewModel.CustomerEmail = invoice.CustomerEmail;
                    viewModel.FuelType = invoice.FuelType;
                    viewModel.Quantity = invoice.TotalLiters;
                    viewModel.UnitPrice = invoice.UnitPrice;
                    viewModel.InvoiceId = invoice.Id;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var receipt = await _context.Receipts.FindAsync(id);
                    if (receipt == null)
                    {
                        return NotFound();
                    }

                    // Check if Invoice Reference already exists (excluding current receipt)
                    var existingInvoiceRef = await _context.Receipts
                        .FirstOrDefaultAsync(r => r.InvoiceRef == viewModel.InvoiceRef && r.Id != id);

                    if (existingInvoiceRef != null)
                    {
                        ModelState.AddModelError("InvoiceRef", "Invoice reference already exists.");
                        viewModel.Invoices = await GetInvoicesSelectList();
                        return View(viewModel);
                    }

                    // Check if Phone Number already exists (excluding current receipt)
                    if (!string.IsNullOrEmpty(viewModel.CustomerPhone))
                    {
                        var existingPhone = await _context.Receipts
                            .FirstOrDefaultAsync(r => r.CustomerPhone == viewModel.CustomerPhone && r.Id != id);

                        if (existingPhone != null)
                        {
                            ModelState.AddModelError("CustomerPhone", "Phone number already exists.");
                            viewModel.Invoices = await GetInvoicesSelectList();
                            return View(viewModel);
                        }
                    }

                    // Store old values for notification
                    var oldReceiptNo = receipt.ReceiptNo;
                    var oldCustomerName = receipt.CustomerName;
                    var oldAmount = receipt.TotalAmount;

                    // Calculate amounts
                    var subtotal = viewModel.Quantity * viewModel.UnitPrice;
                    var vat = subtotal * VAT_RATE;
                    var totalAmount = subtotal + vat;

                    // Update receipt
                    receipt.InvoiceRef = viewModel.InvoiceRef;
                    receipt.InvoiceId = viewModel.InvoiceId;
                    receipt.CustomerName = viewModel.CustomerName;
                    receipt.CustomerType = viewModel.CustomerType;
                    receipt.CustomerPhone = viewModel.CustomerPhone;
                    receipt.CustomerEmail = viewModel.CustomerEmail;
                    receipt.FuelType = viewModel.FuelType;
                    receipt.Quantity = viewModel.Quantity;
                    receipt.UnitPrice = viewModel.UnitPrice;
                    receipt.Subtotal = subtotal;
                    receipt.VAT = vat;
                    receipt.TotalAmount = totalAmount;
                    receipt.AmountPaid = viewModel.AmountPaid;
                    receipt.PaymentMethod = viewModel.PaymentMethod;
                    receipt.PaymentStatus = viewModel.PaymentStatus;
                    receipt.PaymentDate = viewModel.PaymentDate;
                    receipt.Notes = viewModel.Notes;
                    receipt.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // ===== NOTIFICATION: Receipt Updated =====
                    await _notificationService.CreateNotificationAsync(
                        title: $"Receipt Updated: {receipt.ReceiptNo}",
                        description: $"Receipt for {receipt.CustomerName} has been updated",
                        module: "Receipts",
                        type: NotificationType.Info,
                        actionUrl: $"/Receipts/Details/{receipt.Id}",
                        actionText: "View Receipt"
                    );
                    // =========================================

                    TempData["Success"] = "Receipt updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReceiptExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed - redisplay form
            viewModel.Invoices = await GetInvoicesSelectList();
            return View(viewModel);
        }

        // POST: Receipts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var receipt = await _context.Receipts.FindAsync(id);
            if (receipt == null)
            {
                return NotFound();
            }

            var receiptInfo = $"{receipt.ReceiptNo} - {receipt.CustomerName}";

            _context.Receipts.Remove(receipt);
            await _context.SaveChangesAsync();

            // ===== NOTIFICATION: Receipt Deleted =====
            await _notificationService.CreateNotificationAsync(
                title: $"Receipt Deleted: {receipt.ReceiptNo}",
                description: $"Receipt for {receipt.CustomerName} has been deleted",
                module: "Receipts",
                type: NotificationType.Warning,
                actionUrl: "/Receipts",
                actionText: "View Receipts"
            );
            // =========================================

            TempData["Success"] = "Receipt deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // REMOTE VALIDATION METHODS
        // =========================

        // Remote validation for Invoice Reference (CREATE)
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckInvoiceReference(string invoiceRef)
        {
            if (string.IsNullOrEmpty(invoiceRef))
                return Json(true);

            var exists = await _context.Receipts
                .AnyAsync(r => r.InvoiceRef == invoiceRef);

            if (exists)
            {
                return Json($"Invoice reference {invoiceRef} already exists.");
            }

            return Json(true);
        }

        // Remote validation for Invoice Reference (EDIT)
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckInvoiceReferenceForEdit(string invoiceRef, int id)
        {
            if (string.IsNullOrEmpty(invoiceRef))
                return Json(true);

            var exists = await _context.Receipts
                .AnyAsync(r => r.InvoiceRef == invoiceRef && r.Id != id);

            if (exists)
            {
                return Json($"Invoice reference {invoiceRef} already exists.");
            }

            return Json(true);
        }

        // Remote validation for Customer Phone (CREATE)
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckCustomerPhone(string customerPhone)
        {
            if (string.IsNullOrEmpty(customerPhone))
                return Json(true);

            var exists = await _context.Receipts
                .AnyAsync(r => r.CustomerPhone == customerPhone);

            if (exists)
            {
                return Json("Phone number already exists.");
            }

            return Json(true);
        }

        // Remote validation for Customer Phone (EDIT)
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckCustomerPhoneForEdit(string customerPhone, int id)
        {
            if (string.IsNullOrEmpty(customerPhone))
                return Json(true);

            var exists = await _context.Receipts
                .AnyAsync(r => r.CustomerPhone == customerPhone && r.Id != id);

            if (exists)
            {
                return Json("Phone number already exists.");
            }

            return Json(true);
        }

        // =========================
        // API endpoint to get invoice details
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return NotFound();
            }

            return Json(new
            {
                invoiceRef = invoice.InvoiceNumber,
                customerName = invoice.CustomerName,
                customerType = invoice.CustomerType,
                customerPhone = invoice.CustomerPhone,
                customerEmail = invoice.CustomerEmail,
                fuelType = invoice.FuelType,
                quantity = invoice.TotalLiters,
                unitPrice = invoice.UnitPrice
            });
        }

        // =========================
        // HELPER METHODS
        // =========================
        private async Task<List<SelectListItem>> GetInvoicesSelectList()
        {
            var invoices = await _context.Invoices
                .OrderBy(i => i.InvoiceNumber)
                .Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = $"{i.InvoiceNumber} - {i.CustomerName} (${i.TotalAmount})"
                })
                .ToListAsync();

            invoices.Insert(0, new SelectListItem { Value = "", Text = "-- Select Invoice --" });
            return invoices;
        }

        private string DeterminePaymentStatus(decimal totalAmount, decimal? amountPaid)
        {
            if (!amountPaid.HasValue || amountPaid.Value == 0)
                return "Pending";

            if (amountPaid.Value >= totalAmount)
                return "Paid";

            return "Partial";
        }

        private bool ReceiptExists(int id)
        {
            return _context.Receipts.Any(e => e.Id == id);
        }
    }
}