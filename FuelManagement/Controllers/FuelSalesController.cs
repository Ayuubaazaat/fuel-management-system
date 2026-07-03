using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FuelManagement.Data;
using FuelManagement.Models;
using FuelManagement.Models.Enums;
using FuelManagement.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FuelManagement.Controllers
{
    public class FuelSalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private const int PageSizeDefault = 10;
        private const decimal VAT_RATE = 0.05m; // 5% Somali VAT

        public FuelSalesController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ===================== INDEX =====================
        public IActionResult Index(
            int pageNumber = 1,
            int pageSize = PageSizeDefault,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string pumpNumber = null,
            string paymentMethod = null,
            string status = null)
        {
            var today = DateTime.UtcNow.Date;
            var thirtyDaysAgo = today.AddDays(-30);

            // Use provided filters or defaults
            var filterStartDate = startDate ?? thirtyDaysAgo;
            var filterEndDate = endDate ?? today;

            // Get ALL sales for context (for comparisons)
            var allSales = _context.FuelSales.ToList();

            // Build the query with filters
            var query = _context.FuelSales
                .Where(s => s.Date >= filterStartDate && s.Date <= filterEndDate);

            // Apply pump filter
            if (!string.IsNullOrEmpty(pumpNumber))
            {
                query = query.Where(s => s.PumpNumber == pumpNumber);
            }

            // Apply payment method filter
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                var paymentMethodEnum = Enum.Parse<PaymentMethod>(paymentMethod);
                query = query.Where(s => s.PaymentMethod == paymentMethodEnum);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                var statusEnum = Enum.Parse<FuelSaleStatus>(status);
                query = query.Where(s => s.Status == statusEnum);
            }

            // Get current period sales query (for pagination) - ORDER BY INVOICE NUMBER ASCENDING
            var currentPeriodSalesQuery = query
                .OrderBy(s => s.InvoiceNumber)
                .ThenBy(s => s.CustomerName);

            // Get total count for pagination
            var totalCount = currentPeriodSalesQuery.Count();

            // Apply pagination
            var currentPeriodSales = currentPeriodSalesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new FuelSaleListItemViewModel
                {
                    Id = s.Id,
                    InvoiceNumber = s.InvoiceNumber,
                    CustomerName = s.CustomerName,
                    Liters = s.Liters,
                    Amount = s.Amount,
                    Date = s.Date,
                    PumpNumber = s.PumpNumber,
                    PaymentMethod = s.PaymentMethod.ToString(),
                    Status = s.Status.ToString()
                })
                .ToList();

            // Get previous period - use the same length as current period but from earlier
            DateTime earliestCurrentDate;
            if (currentPeriodSalesQuery.Any())
            {
                earliestCurrentDate = currentPeriodSalesQuery.Min(s => s.Date);
            }
            else
            {
                earliestCurrentDate = today;
            }

            var previousPeriodStart = earliestCurrentDate.AddDays(-30);
            var previousPeriodEnd = earliestCurrentDate.AddDays(-1);

            var previousPeriodSales = _context.FuelSales
                .Where(s => s.Date >= previousPeriodStart && s.Date <= previousPeriodEnd)
                .ToList();

            // Calculate current period metrics (using ALL filtered data, not just paginated)
            var allCurrentPeriodData = currentPeriodSalesQuery.ToList();
            var totalSalesAmount = allCurrentPeriodData.Sum(s => s.Amount);
            var totalLitersSold = allCurrentPeriodData.Sum(s => s.Liters);
            var totalTransactions = allCurrentPeriodData.Count;
            var todaysRevenue = allCurrentPeriodData.Where(s => s.Date.Date == today).Sum(s => s.Amount);
            var averageTransaction = totalTransactions > 0 ? totalSalesAmount / totalTransactions : 0;
            var pendingInvoices = allCurrentPeriodData.Count(s => s.Status == FuelSaleStatus.Pending);

            // Calculate previous period metrics
            var previousTotalAmount = previousPeriodSales.Sum(s => s.Amount);
            var previousTotalLiters = previousPeriodSales.Sum(s => s.Liters);
            var previousPendingCount = previousPeriodSales.Count(s => s.Status == FuelSaleStatus.Pending);

            // Calculate average daily revenue (based on ALL historical data for better comparison)
            var allTimeDaysWithSales = allSales.Select(s => s.Date.Date).Distinct().Count();
            var allTimeAverageDailyRevenue = allTimeDaysWithSales > 0
                ? allSales.Sum(s => s.Amount) / allTimeDaysWithSales
                : 0;

            // Calculate professional percentages
            var salesGrowthPercentage = CalculateProfessionalPercentage(totalSalesAmount, previousTotalAmount, "amount");
            var litersGrowthPercentage = CalculateProfessionalPercentage(totalLitersSold, previousTotalLiters, "liters");
            var revenueVsAveragePercentage = CalculateProfessionalPercentage(todaysRevenue, allTimeAverageDailyRevenue, "revenue");
            var pendingChangePercentage = CalculateProfessionalPercentage(pendingInvoices, previousPendingCount, "pending");

            var viewModel = new FuelSaleViewModel
            {
                Filter = new FuelSaleFilterViewModel
                {
                    StartDate = filterStartDate,
                    EndDate = filterEndDate,
                    PumpNumber = pumpNumber,
                    PaymentMethod = paymentMethod,
                    Status = status
                },
                Sales = currentPeriodSales,

                // Summary data
                TotalSalesAmount = totalSalesAmount,
                TotalLitersSold = totalLitersSold,
                TotalTransactions = totalTransactions,
                TodaysRevenue = todaysRevenue,
                AverageTransaction = averageTransaction,
                PendingInvoices = pendingInvoices,

                // Comparison data
                LastMonthSalesAmount = previousTotalAmount,
                LastMonthLitersSold = previousTotalLiters,
                AverageDailyRevenue = allTimeAverageDailyRevenue,
                PreviousPendingInvoices = previousPendingCount,

                // Calculated percentages
                SalesGrowthPercentage = salesGrowthPercentage.RawValue,
                LitersGrowthPercentage = litersGrowthPercentage.RawValue,
                RevenueVsAveragePercentage = revenueVsAveragePercentage.RawValue,
                PendingChangePercentage = pendingChangePercentage.RawValue,

                // Pagination data
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            // Set ViewBag for formatted percentages
            ViewBag.SalesGrowthValue = salesGrowthPercentage.FormattedValue;
            ViewBag.SalesGrowthRaw = salesGrowthPercentage.RawValue;
            ViewBag.SalesGrowthIsNew = salesGrowthPercentage.IsNew;

            ViewBag.LitersGrowthValue = litersGrowthPercentage.FormattedValue;
            ViewBag.LitersGrowthRaw = litersGrowthPercentage.RawValue;
            ViewBag.LitersGrowthIsNew = litersGrowthPercentage.IsNew;

            ViewBag.RevenueVsAverageValue = revenueVsAveragePercentage.FormattedValue;
            ViewBag.RevenueVsAverageRaw = revenueVsAveragePercentage.RawValue;
            ViewBag.RevenueVsAverageIsNew = revenueVsAveragePercentage.IsNew;

            ViewBag.PendingChangeValue = pendingChangePercentage.FormattedValue;
            ViewBag.PendingChangeRaw = pendingChangePercentage.RawValue;
            ViewBag.PendingChangeIsNew = pendingChangePercentage.IsNew;

            return View(viewModel);
        }

        private (string FormattedValue, decimal RawValue, bool IsNew) CalculateProfessionalPercentage(
            decimal current,
            decimal previous,
            string metricType)
        {
            // Case 1: First entry in this category
            if (previous == 0 && current > 0)
            {
                if (metricType == "pending")
                {
                    return ($"{current} pending", current, true);
                }
                else if (metricType == "revenue")
                {
                    return ($"+{current.ToString("C")}", current, true);
                }
                else
                {
                    return ($"+{current}", current, true);
                }
            }

            // Case 2: For revenue vs average, show dollar difference when small
            if (metricType == "revenue" && Math.Abs(current - previous) < 10)
            {
                var diff = current - previous;
                if (diff > 0)
                    return ($"+{diff.ToString("C")}", diff, false);
                else if (diff < 0)
                    return ($"-{Math.Abs(diff).ToString("C")}", diff, false);
            }

            // Case 3: Normal percentage calculation for meaningful differences
            if (previous > 0 && Math.Abs(current - previous) / previous >= 0.05m)
            {
                var change = ((current - previous) / previous) * 100;
                change = Math.Round(change, 1);

                if (change > 0)
                    return ($"+{change}%", change, false);
                else if (change < 0)
                    return ($"{change}%", change, false);
            }

            // Case 4: Small changes or equal values
            if (current == previous)
                return ("0%", 0, false);

            var difference = current - previous;
            if (difference > 0)
                return ($"+{difference.ToString("N1")}", difference, false);
            else
                return ($"-{Math.Abs(difference).ToString("N1")}", difference, false);
        }

        private (string FormattedValue, decimal RawValue, bool IsNew) CalculateProfessionalPercentage(
            int current,
            int previous,
            string metricType)
        {
            // Case 1: First entry in this category
            if (previous == 0 && current > 0)
            {
                if (metricType == "pending")
                {
                    return ($"{current} pending", current, true);
                }
                else
                {
                    return ($"+{current}", current, true);
                }
            }

            // Case 2: No change
            if (current == previous)
                return ("0%", 0, false);

            // Case 3: Normal percentage calculation for meaningful differences
            if (previous > 0)
            {
                // Only show percentage if change is significant (>5%)
                if (Math.Abs(current - previous) / (decimal)previous >= 0.05m)
                {
                    var change = ((decimal)(current - previous) / previous) * 100;
                    change = Math.Round(change, 1);

                    if (change > 0)
                        return ($"+{change}%", change, false);
                    else if (change < 0)
                        return ($"{change}%", change, false);
                }

                // Small change, show actual number difference
                var difference = current - previous;
                if (difference > 0)
                    return ($"+{difference}", difference, false);
                else
                    return ($"{difference}", difference, false);
            }

            return ("0%", 0, false);
        }

        // ===================== CREATE =====================
        public IActionResult Create()
        {
            // Generate next invoice number
            var lastInvoice = _context.FuelSales
                .OrderByDescending(s => s.Id)
                .Select(s => s.InvoiceNumber)
                .FirstOrDefault();

            string nextInvoiceNumber;
            if (string.IsNullOrEmpty(lastInvoice))
            {
                nextInvoiceNumber = "INV-001";
            }
            else
            {
                // Handle both "INV-001" and "inv-001" formats
                var hyphenIndex = lastInvoice.LastIndexOf('-');
                if (hyphenIndex >= 0 && hyphenIndex < lastInvoice.Length - 1)
                {
                    var numberPart = lastInvoice.Substring(hyphenIndex + 1);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextInvoiceNumber = $"INV-{(lastNumber + 1):D3}";
                    }
                    else
                    {
                        nextInvoiceNumber = "INV-001";
                    }
                }
                else
                {
                    nextInvoiceNumber = "INV-001";
                }
            }

            var viewModel = new FuelSaleCreateViewModel
            {
                InvoiceNumber = nextInvoiceNumber,
                Date = DateTime.UtcNow.Date
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FuelSaleCreateViewModel viewModel)
        {
            // Check if invoice number already exists (do this BEFORE ModelState validation)
            var existingSale = _context.FuelSales
                .FirstOrDefault(s => s.InvoiceNumber == viewModel.InvoiceNumber);

            if (existingSale != null)
            {
                ModelState.AddModelError("InvoiceNumber", "Unique Invoice Number Allowed");
                return View(viewModel);
            }

            if (!ModelState.IsValid)
                return View(viewModel);

            var sale = new FuelSale
            {
                InvoiceNumber = viewModel.InvoiceNumber,
                CustomerName = viewModel.CustomerName,
                Liters = viewModel.Liters,
                PricePerLiter = viewModel.PricePerLiter,
                Amount = viewModel.Amount,
                Date = DateTime.SpecifyKind(viewModel.Date, DateTimeKind.Utc),
                PumpNumber = viewModel.PumpNumber,
                PaymentMethod = Enum.Parse<PaymentMethod>(viewModel.PaymentMethod),
                Status = FuelSaleStatus.Pending,
                Notes = viewModel.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.FuelSales.Add(sale);
            await _context.SaveChangesAsync();

            // ===== ADD SUCCESS MESSAGE =====
            TempData["Success"] = $"Fuel sale {sale.InvoiceNumber} created successfully!";
            // ===============================

            // ===== CREATE NOTIFICATION FOR NEW FUEL SALE =====
            await _notificationService.CreateFuelSaleNotification(
                invoiceNumber: sale.InvoiceNumber,
                customerName: sale.CustomerName,
                liters: sale.Liters,
                amount: sale.Amount,
                actionUrl: $"/FuelSales/Details/{sale.Id}"
            );
            // =================================================

            // ===== AUTO-CREATE INVOICE FROM FUEL SALE =====
            await CreateInvoiceFromFuelSale(sale);
            // ==============================================

            return RedirectToAction(nameof(Index));
        }

        // ===================== EDIT =====================
        public IActionResult Edit(int id)
        {
            var sale = _context.FuelSales.FirstOrDefault(s => s.Id == id);
            if (sale == null)
                return NotFound();

            var viewModel = new FuelSaleEditViewModel
            {
                Id = sale.Id,
                InvoiceNumber = sale.InvoiceNumber,
                CustomerName = sale.CustomerName,
                Liters = sale.Liters,
                PricePerLiter = sale.PricePerLiter,
                Date = sale.Date,
                PumpNumber = sale.PumpNumber,
                PaymentMethod = sale.PaymentMethod.ToString(),
                Status = sale.Status.ToString(),
                Notes = sale.Notes
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FuelSaleEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var sale = _context.FuelSales.FirstOrDefault(s => s.Id == viewModel.Id);
            if (sale == null)
                return NotFound();

            // Check if invoice number is being changed and if it already exists
            if (sale.InvoiceNumber != viewModel.InvoiceNumber)
            {
                var existingInvoice = _context.FuelSales
                    .FirstOrDefault(s => s.InvoiceNumber == viewModel.InvoiceNumber && s.Id != viewModel.Id);

                if (existingInvoice != null)
                {
                    ModelState.AddModelError("InvoiceNumber", "Unique Invoice Number Allowed");
                    return View(viewModel);
                }
            }

            sale.InvoiceNumber = viewModel.InvoiceNumber;
            sale.CustomerName = viewModel.CustomerName;
            sale.Liters = viewModel.Liters;
            sale.PricePerLiter = viewModel.PricePerLiter;
            sale.Amount = viewModel.Amount;
            sale.Date = DateTime.SpecifyKind(viewModel.Date, DateTimeKind.Utc);
            sale.PumpNumber = viewModel.PumpNumber;
            sale.PaymentMethod = Enum.Parse<PaymentMethod>(viewModel.PaymentMethod);
            sale.Status = Enum.Parse<FuelSaleStatus>(viewModel.Status);
            sale.Notes = viewModel.Notes;

            await _context.SaveChangesAsync();

            // ===== ADD SUCCESS MESSAGE =====
            TempData["Success"] = $"Fuel sale {sale.InvoiceNumber} updated successfully!";
            // ===============================

            // ===== CREATE NOTIFICATION FOR UPDATED FUEL SALE =====
            await _notificationService.CreateNotificationAsync(
                title: $"Fuel Sale Updated: {sale.InvoiceNumber}",
                description: $"Sale for {sale.CustomerName} has been updated",
                module: "Fuel Sales",
                type: NotificationType.Info,
                actionUrl: $"/FuelSales/Details/{sale.Id}",
                actionText: "View Sale"
            );
            // =====================================================

            // ===== UPDATE LINKED INVOICE IF STATUS CHANGED =====
            await UpdateInvoiceFromFuelSale(sale);
            // ===================================================

            return RedirectToAction(nameof(Index));
        }

        // ===================== DETAILS =====================
        public IActionResult Details(int id)
        {
            var sale = _context.FuelSales.FirstOrDefault(s => s.Id == id);
            if (sale == null)
                return NotFound();

            var viewModel = new FuelSaleListItemViewModel
            {
                Id = sale.Id,
                InvoiceNumber = sale.InvoiceNumber,
                CustomerName = sale.CustomerName,
                Liters = sale.Liters,
                Amount = sale.Amount,
                Date = sale.Date,
                PumpNumber = sale.PumpNumber,
                PaymentMethod = sale.PaymentMethod.ToString(),
                Status = sale.Status.ToString()
            };

            return View(viewModel);
        }

        // ===================== DELETE =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sale = _context.FuelSales.FirstOrDefault(s => s.Id == id);
            if (sale != null)
            {
                // ===== CHECK FOR LINKED INVOICE AND ITS RECEIPTS =====
                var linkedInvoice = _context.Invoices.FirstOrDefault(i => i.FuelSaleId == id);
                if (linkedInvoice != null)
                {
                    // Check if this invoice has any receipts
                    var hasReceipts = _context.Receipts.Any(r => r.InvoiceId == linkedInvoice.Id || r.InvoiceRef == linkedInvoice.InvoiceNumber);

                    if (hasReceipts)
                    {
                        TempData["Error"] = $"Cannot delete fuel sale because invoice {linkedInvoice.InvoiceNumber} has receipts linked to it. Delete the receipts first.";
                        return RedirectToAction(nameof(Index));
                    }

                    // If no receipts, safe to delete the invoice
                    _context.Invoices.Remove(linkedInvoice);
                }
                // =====================================================

                _context.FuelSales.Remove(sale);
                await _context.SaveChangesAsync();

                // ===== CREATE NOTIFICATION FOR DELETED FUEL SALE =====
                await _notificationService.CreateNotificationAsync(
                    title: $"Fuel Sale Deleted: {sale.InvoiceNumber}",
                    description: $"Sale for {sale.CustomerName} has been deleted",
                    module: "Fuel Sales",
                    type: NotificationType.Warning,
                    actionUrl: "/FuelSales",
                    actionText: "View Sales"
                );
                // =====================================================

                TempData["Success"] = "Fuel sale deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ===================== HELPER METHODS =====================
        private async Task CreateInvoiceFromFuelSale(FuelSale sale)
        {
            try
            {
                // Determine customer type based on customer name patterns
                string customerType = DetermineCustomerType(sale.CustomerName);

                // Calculate invoice values
                var subtotal = sale.Amount;
                var vat = subtotal * VAT_RATE;
                var totalAmount = subtotal + vat;

                // Determine payment status based on fuel sale status
                string paymentStatus = sale.Status switch
                {
                    FuelSaleStatus.Paid => "Paid",
                    FuelSaleStatus.Pending => "Unpaid",
                    FuelSaleStatus.Cancelled => "Unpaid",
                    _ => "Unpaid"
                };

                // Determine amount paid (if status is Paid, assume full payment)
                decimal amountPaid = sale.Status == FuelSaleStatus.Paid ? totalAmount : 0;
                decimal balanceDue = totalAmount - amountPaid;

                var invoice = new Invoice
                {
                    InvoiceNumber = sale.InvoiceNumber,
                    CustomerName = sale.CustomerName,
                    CustomerType = customerType,
                    CustomerPhone = null, // No phone in fuel sale
                    CustomerEmail = null, // No email in fuel sale
                    CustomerAddress = null, // No address in fuel sale
                    FuelType = DetermineFuelTypeFromPump(sale.PumpNumber), // You might need a way to determine fuel type
                    TotalLiters = sale.Liters,
                    UnitPrice = sale.PricePerLiter,
                    Subtotal = subtotal,
                    VAT = vat,
                    TotalAmount = totalAmount,
                    AmountPaid = amountPaid,
                    BalanceDue = balanceDue,
                    PaymentStatus = paymentStatus,
                    IssueDate = sale.Date,
                    DueDate = sale.Date.AddDays(30), // 30 days payment terms
                    CreatedBy = "System (Auto from Fuel Sale)",
                    CreatedAt = DateTime.UtcNow,
                    Notes = $"Auto-generated from fuel sale. Payment method: {sale.PaymentMethod}, Pump: {sale.PumpNumber}",
                    FuelSaleId = sale.Id
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Invoice {invoice.InvoiceNumber} created automatically from fuel sale {sale.Id}");
            }
            catch (Exception ex)
            {
                // Log error but don't fail the fuel sale creation
                Console.WriteLine($"? Failed to create invoice from fuel sale: {ex.Message}");
            }
        }

        private async Task UpdateInvoiceFromFuelSale(FuelSale sale)
        {
            try
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.FuelSaleId == sale.Id);
                if (invoice == null)
                    return;

                // Recalculate invoice values
                var subtotal = sale.Amount;
                var vat = subtotal * VAT_RATE;
                var totalAmount = subtotal + vat;

                // Update payment status based on fuel sale status
                string paymentStatus = sale.Status switch
                {
                    FuelSaleStatus.Paid => "Paid",
                    FuelSaleStatus.Pending => "Unpaid",
                    FuelSaleStatus.Cancelled => "Unpaid",
                    _ => invoice.PaymentStatus
                };

                decimal amountPaid = sale.Status == FuelSaleStatus.Paid ? totalAmount : invoice.AmountPaid;
                decimal balanceDue = totalAmount - amountPaid;

                // Update invoice properties
                invoice.CustomerName = sale.CustomerName;
                invoice.TotalLiters = sale.Liters;
                invoice.UnitPrice = sale.PricePerLiter;
                invoice.Subtotal = subtotal;
                invoice.VAT = vat;
                invoice.TotalAmount = totalAmount;
                invoice.AmountPaid = amountPaid;
                invoice.BalanceDue = balanceDue;
                invoice.PaymentStatus = paymentStatus;
                invoice.UpdatedAt = DateTime.UtcNow;
                invoice.Notes = $"Updated from fuel sale. Payment method: {sale.PaymentMethod}, Pump: {sale.PumpNumber}";

                await _context.SaveChangesAsync();

                Console.WriteLine($"? Invoice {invoice.InvoiceNumber} updated from fuel sale {sale.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Failed to update invoice from fuel sale: {ex.Message}");
            }
        }

        private string DetermineCustomerType(string customerName)
        {
            // Simple logic to determine customer type
            // You can expand this based on your business rules
            if (customerName.ToLower().Contains("fleet") || customerName.ToLower().Contains("transport"))
                return "Fleet";
            else if (customerName.ToLower().Contains("ltd") || customerName.ToLower().Contains("company"))
                return "Corporate";
            else
                return "Walk-in";
        }

        private string DetermineFuelTypeFromPump(string pumpNumber)
        {
            // Simple logic to determine fuel type based on pump number
            // You can expand this based on your actual pump configuration
            return pumpNumber switch
            {
                "P-01" => "Premium Gasoline",
                "P-02" => "Regular Gasoline",
                "P-03" => "Diesel",
                "P-04" => "Premium Diesel",
                "P-05" => "Ethanol",
                _ => "Premium Gasoline"
            };
        }
    }
}