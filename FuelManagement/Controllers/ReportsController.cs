using Microsoft.AspNetCore.Mvc;
using FuelManagement.Models;
using FuelManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FuelManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const decimal VAT_RATE = 0.05m; // 5% Somali VAT
        private const int PageSize = 10; // Show 10 rows per page

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(ReportFilterViewModel filter, int pageNumber = 1)
        {
            // Set default filter if not provided
            if (filter == null)
            {
                filter = new ReportFilterViewModel
                {
                    StartDate = DateTime.UtcNow.Date.AddMonths(-12),
                    EndDate = DateTime.UtcNow.Date,
                    ReportType = "Revenue by Month"
                };
            }

            // Get all invoices
            var allInvoices = await _context.Invoices.ToListAsync();

            // Log the count for debugging
            Console.WriteLine($"Total invoices in database: {allInvoices.Count}");

            // Apply date filter if dates are provided
            var invoicesQuery = _context.Invoices.AsQueryable();

            if (filter.StartDate.HasValue)
            {
                invoicesQuery = invoicesQuery.Where(i => i.IssueDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                // Add one day to include the end date fully
                var endDate = filter.EndDate.Value.AddDays(1);
                invoicesQuery = invoicesQuery.Where(i => i.IssueDate < endDate);
            }

            var invoices = await invoicesQuery.ToListAsync();

            // Calculate summary statistics from ALL invoices
            var totalRevenue = allInvoices.Sum(i => i.TotalAmount);
            var totalVAT = allInvoices.Sum(i => i.VAT);
            var totalOutstanding = allInvoices.Sum(i => i.BalanceDue);
            var totalInvoices = allInvoices.Count;

            // Handle null/empty payment status
            var paidInvoices = allInvoices.Count(i =>
                !string.IsNullOrEmpty(i.PaymentStatus) &&
                (i.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)));

            var overdueInvoices = allInvoices.Count(i =>
                !string.IsNullOrEmpty(i.PaymentStatus) &&
                i.PaymentStatus.Equals("Overdue", StringComparison.OrdinalIgnoreCase));

            // Calculate paid amount for collection rate
            var paidAmount = allInvoices
                .Where(i => !string.IsNullOrEmpty(i.PaymentStatus) &&
                           (i.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)))
                .Sum(i => i.TotalAmount);

            var collectionRate = totalRevenue > 0
                ? Math.Round((paidAmount / totalRevenue) * 100, 1)
                : 0;

            // Generate report data - ONE ROW PER INVOICE for the table
            var reportItems = new List<ReportListItemViewModel>();
            var months = new[] { "January", "February", "March", "April", "May", "June",
                                  "July", "August", "September", "October", "November", "December" };

            var chartLabels = new List<string>();
            var revenueData = new List<decimal>();
            var vatData = new List<decimal>();
            var outstandingData = new List<decimal>();

            // Sort invoices by date for display
            var sortedInvoices = allInvoices.OrderBy(i => i.IssueDate).ToList();

            // Create one row per invoice for the table
            foreach (var invoice in sortedInvoices)
            {
                var monthlyRevenue = invoice.TotalAmount;
                var monthlyVAT = invoice.VAT;
                var monthlyOutstanding = invoice.BalanceDue;
                var monthlyPaidInvoices = !string.IsNullOrEmpty(invoice.PaymentStatus) &&
                                          invoice.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                // Determine status based on outstanding ratio
                var status = "Healthy";
                if (monthlyRevenue > 0)
                {
                    status = (monthlyOutstanding / monthlyRevenue) < 0.1m ? "Healthy" : "Warning";
                }

                reportItems.Add(new ReportListItemViewModel
                {
                    Id = invoice.Id,
                    Month = invoice.IssueDate.ToString("MMMM"),
                    Year = invoice.IssueDate.Year,
                    Revenue = monthlyRevenue,
                    VAT = monthlyVAT,
                    Outstanding = monthlyOutstanding,
                    TotalInvoices = 1,
                    PaidInvoices = monthlyPaidInvoices,
                    Status = status
                });

                // For chart data, still group by month
                var monthKey = $"{months[invoice.IssueDate.Month - 1].Substring(0, 3)} {invoice.IssueDate.Year.ToString().Substring(2, 2)}";
                var existingIndex = chartLabels.FindIndex(l => l == monthKey);

                if (existingIndex >= 0)
                {
                    // Add to existing month
                    revenueData[existingIndex] += monthlyRevenue;
                    vatData[existingIndex] += monthlyVAT;
                    outstandingData[existingIndex] += monthlyOutstanding;
                }
                else
                {
                    // New month
                    chartLabels.Add(monthKey);
                    revenueData.Add(monthlyRevenue);
                    vatData.Add(monthlyVAT);
                    outstandingData.Add(monthlyOutstanding);
                }
            }

            // Sort report items by date (newest first)
            reportItems = reportItems.OrderByDescending(x => x.Year).ThenByDescending(x =>
                Array.IndexOf(months, x.Month)).ToList();

            // Apply pagination
            var totalCount = reportItems.Count;
            var pagedReportItems = reportItems
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var viewModel = new ReportsViewModel
            {
                Filter = filter,
                Summary = new ReportSummaryViewModel
                {
                    TotalRevenue = totalRevenue,
                    TotalVATCollected = totalVAT,
                    TotalOutstanding = totalOutstanding,
                    CollectionRate = collectionRate,
                    TotalInvoices = totalInvoices,
                    PaidInvoices = paidInvoices,
                    OverdueInvoices = overdueInvoices
                },
                ChartData = new ReportChartViewModel
                {
                    Labels = chartLabels,
                    RevenueData = revenueData,
                    VATData = vatData,
                    OutstandingData = outstandingData
                },
                ReportItems = pagedReportItems
            };

            // Store pagination info in ViewBag
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ViewBag.CurrentFilter = filter;

            return View(viewModel);
        }

        public IActionResult Details(int id)
        {
            // For future detailed report views
            return View();
        }

        //export
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(string format, ReportFilterViewModel filter)
        {
            try
            {
                // Get the data to export
                var allInvoices = _context.Invoices.ToList();

                // Apply filters if provided
                if (filter.StartDate.HasValue)
                    allInvoices = allInvoices.Where(i => i.IssueDate >= filter.StartDate.Value).ToList();

                if (filter.EndDate.HasValue)
                    allInvoices = allInvoices.Where(i => i.IssueDate <= filter.EndDate.Value).ToList();

                // Handle PDF export (coming soon)
                if (format.ToLower() == "pdf")
                {
                    TempData["Info"] = "PDF export feature coming soon!";
                    return RedirectToAction(nameof(Index));
                }
                // Handle Excel/CSV export
                else if (format.ToLower() == "excel")
                {
                    // Generate CSV content
                    var csv = GenerateCsv(allInvoices);

                    // Convert to bytes for download
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

                    // Create filename with timestamp
                    var fileName = $"Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                    // Return file for download
                    return File(bytes, "text/csv", fileName);
                }

                // Invalid format
                TempData["Error"] = "Invalid export format.";
            }
            catch (Exception ex)
            {
                // Handle any errors
                TempData["Error"] = $"Export failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Generates a CSV string from a list of invoices
        /// </summary>
        private string GenerateCsv(List<Invoice> invoices)
        {
            var sb = new System.Text.StringBuilder();

            // Add CSV headers
            sb.AppendLine("Invoice Number,Customer Name,Fuel Type,Liters,Unit Price,Subtotal,VAT,Total,Amount Paid,Balance Due,Status,Issue Date,Due Date");

            // Add data rows
            foreach (var invoice in invoices)
            {
                sb.AppendLine($"{invoice.InvoiceNumber}," +                      // Invoice Number
                              $"\"{invoice.CustomerName}\"," +                   // Customer Name (in quotes to handle commas)
                              $"{invoice.FuelType}," +                           // Fuel Type
                              $"{invoice.TotalLiters}," +                        // Liters
                              $"{invoice.UnitPrice}," +                          // Unit Price
                              $"{invoice.Subtotal}," +                           // Subtotal
                              $"{invoice.VAT}," +                                // VAT
                              $"{invoice.TotalAmount}," +                        // Total Amount
                              $"{invoice.AmountPaid}," +                         // Amount Paid
                              $"{invoice.BalanceDue}," +                         // Balance Due
                              $"{invoice.PaymentStatus}," +                      // Payment Status
                              $"{invoice.IssueDate:yyyy-MM-dd}," +               // Issue Date
                              $"{invoice.DueDate:yyyy-MM-dd}");                  // Due Date
            }

            return sb.ToString();
        }
    }
}