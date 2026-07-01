using FuelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FuelManagement.Data;
using FuelManagement.Models;
using FuelManagement.Models.Enums;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace FuelManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public HomeController(AuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string searchTerm = null)
        {
            // DEBUG: Check authentication state
            var isAuth = _authService.IsAuthenticated();
            var userId = _authService.GetCurrentUserId();
            var userRole = _authService.GetCurrentUserRole();

            Console.WriteLine($"HomeController.Index - IsAuthenticated: {isAuth}, UserId: {userId}, Role: {userRole}");

            // Check if user is authenticated
            if (!isAuth)
            {
                Console.WriteLine("Not authenticated, redirecting to Login");
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Dashboard";
            ViewBag.SearchTerm = searchTerm;
            ViewBag.UserRole = userRole; // Pass role to view

            var today = DateTime.UtcNow.Date;
            var now = DateTime.UtcNow;

            // Apply search filter if provided
            var fuelSalesQuery = _context.FuelSales.AsQueryable();
            var invoicesQuery = _context.Invoices.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                fuelSalesQuery = fuelSalesQuery.Where(f =>
                    f.InvoiceNumber.Contains(searchTerm) ||
                    f.CustomerName.Contains(searchTerm) ||
                    f.PumpNumber.Contains(searchTerm));

                invoicesQuery = invoicesQuery.Where(i =>
                    i.InvoiceNumber.Contains(searchTerm) ||
                    i.CustomerName.Contains(searchTerm));
            }

            // ========== SUMMARY CARDS ==========

            // Total Revenue - All users can see
            var totalRevenue = await invoicesQuery
                .Where(i => i.PaymentStatus == "Paid")
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            // Total Fuel Sales - All users can see
            var totalFuelSales = await fuelSalesQuery
                .SumAsync(f => (decimal?)f.Amount) ?? 0;

            // Total Liters Sold - All users can see
            var totalLitersSold = await fuelSalesQuery
                .SumAsync(f => (decimal?)f.Liters) ?? 0;

            // Active Pumps - All users can see
            var activePumps = await _context.Pumps
                .CountAsync(p => p.Status == "Active");
            var totalPumps = await _context.Pumps.CountAsync();

            // Today's Revenue - All users can see
            var todaysRevenue = await fuelSalesQuery
                .Where(f => f.Date.Date == today)
                .SumAsync(f => (decimal?)f.Amount) ?? 0;

            // Average Transaction - All users can see
            var avgTransaction = await fuelSalesQuery
                .Where(f => f.Date.Date == today)
                .AverageAsync(f => (decimal?)f.Amount) ?? 0;

            // Pending Invoices - All users can see
            var pendingInvoices = await invoicesQuery
                .CountAsync(i => i.PaymentStatus == "Unpaid" || i.PaymentStatus == "Partial");

            // ========== FUEL LOSS/VAR ==========
            var totalFuelSold = await fuelSalesQuery.SumAsync(f => (decimal?)f.Liters) ?? 0;
            var totalInventoryStock = await _context.Inventories.SumAsync(i => (decimal?)i.CurrentStock) ?? 0;

            decimal fuelLossVariance = 0;
            if (totalFuelSold > 0)
            {
                var difference = Math.Abs(totalFuelSold - totalInventoryStock);
                var ratio = difference / totalFuelSold;
                fuelLossVariance = Math.Min(ratio * 100, 100) / 100;
            }

            // Fleet Usage - All users can see
            var totalFuelUsed = await _context.Trips.SumAsync(t => (decimal?)t.FuelUsed) ?? 0;
            var totalDistance = await _context.Trips.SumAsync(t => (decimal?)t.Distance) ?? 0;
            var fleetUsage = totalDistance > 0 ? (totalFuelUsed / totalDistance) * 100 : 0;

            // ========== ACTIVE SESSIONS ==========
            var fuelingNow = await fuelSalesQuery
                .CountAsync(f => f.Date.Date == today && f.Status == FuelSaleStatus.Pending);
            var activePumpCount = await _context.Pumps.CountAsync(p => p.Status == "Active");
            var activeSessions = fuelingNow + activePumpCount;

            // ========== CAR WASH ==========
            var todayCarWashes = await _context.CarWashes
                .CountAsync(c => c.CreatedAt.Date == today);

            var todayCarWashRevenue = await _context.CarWashes
                .Where(c => c.CreatedAt.Date == today && c.PaymentStatus == "Paid")
                .SumAsync(c => (decimal?)c.Price) ?? 0;

            var totalCarWashes = await _context.CarWashes.CountAsync();

            var totalCarWashRevenue = await _context.CarWashes
                .Where(c => c.PaymentStatus == "Paid")
                .SumAsync(c => (decimal?)c.Price) ?? 0;

            var pendingCarWashes = await _context.CarWashes
                .CountAsync(c => c.Status == "Pending" || c.Status == "InProgress");

            // ========== FUEL CLIENTS ==========
            var totalClients = await _context.FuelClients.CountAsync();

            var activeClients = await _context.FuelClients
                .CountAsync(c => c.IsActive == true);

            var newClientsThisMonth = await _context.FuelClients
                .CountAsync(c => c.CreatedAt.Month == today.Month
                              && c.CreatedAt.Year == today.Year);

            // ========== COMPENSATION ==========
            var totalCompensationPaidThisMonth = await _context.Compensations
                .Where(c => c.PaymentDate.Month == today.Month
                         && c.PaymentDate.Year == today.Year
                         && c.PaymentStatus == "Paid")
                .SumAsync(c => (decimal?)c.NetSalary) ?? 0;

            var totalEmployees = await _context.Compensations
                .Select(c => c.EmployeeName)
                .Distinct()
                .CountAsync();

            var pendingCompensations = await _context.Compensations
                .CountAsync(c => c.PaymentStatus == "Pending");

            // ========== GROWTH PERCENTAGES ==========
            var lastMonth = today.AddMonths(-1);
            var lastMonthRevenue = await fuelSalesQuery
                .Where(f => f.Date.Month == lastMonth.Month && f.Date.Year == lastMonth.Year)
                .SumAsync(f => (decimal?)f.Amount) ?? 0;

            var revenueGrowth = lastMonthRevenue > 0
                ? ((totalFuelSales - lastMonthRevenue) / lastMonthRevenue) * 100
                : 0;

            var lastMonthLiters = await fuelSalesQuery
                .Where(f => f.Date.Month == lastMonth.Month && f.Date.Year == lastMonth.Year)
                .SumAsync(f => (decimal?)f.Liters) ?? 0;

            var litersGrowth = lastMonthLiters > 0
                ? ((totalLitersSold - lastMonthLiters) / lastMonthLiters) * 100
                : 0;

            // ========== TODAY'S VS AVERAGE ==========
            var last30Days = today.AddDays(-30);
            var averageDailyRevenue = await fuelSalesQuery
                .Where(f => f.Date.Date >= last30Days && f.Date.Date < today)
                .AverageAsync(f => (decimal?)f.Amount) ?? 1;

            var todaysVsAverage = averageDailyRevenue > 0
                ? ((todaysRevenue - averageDailyRevenue) / averageDailyRevenue) * 100
                : (todaysRevenue > 0 ? 100 : 0);

            // ========== MONTHLY CHART DATA ==========
            var monthlySales = new List<MonthlySalesData>();
            for (int i = 1; i <= 12; i++)
            {
                var sales = await fuelSalesQuery
                    .Where(f => f.Date.Month == i && f.Date.Year == today.Year)
                    .SumAsync(f => (decimal?)f.Amount) ?? 0;

                monthlySales.Add(new MonthlySalesData
                {
                    Month = new DateTime(today.Year, i, 1).ToString("MMM"),
                    Sales = sales
                });
            }

            // ========== WEEKLY CHART DATA (LAST 4 WEEKS) ==========
            var weeklySales = new List<WeeklySalesData>();

            var daysToMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var startOfCurrentWeek = today.AddDays(-daysToMonday);

            for (int i = 4; i >= 1; i--)
            {
                var weekStart = startOfCurrentWeek.AddDays(-(i - 1) * 7);
                var weekEnd = weekStart.AddDays(6);

                var weekSales = await fuelSalesQuery
                    .Where(f => f.Date.Date >= weekStart && f.Date.Date <= weekEnd)
                    .SumAsync(f => (decimal?)f.Amount) ?? 0;

                weeklySales.Add(new WeeklySalesData
                {
                    WeekLabel = $"Week {5 - i}",
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    Sales = weekSales,
                    DisplayLabel = $"{weekStart:MMM dd} - {weekEnd:MMM dd}"
                });
            }

            // ========== RECENT TRANSACTIONS WITH PAGINATION ==========
            var totalTransactions = await fuelSalesQuery.CountAsync();

            var totalPages = (int)Math.Ceiling(totalTransactions / (double)PageSize);

            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var allTransactions = await fuelSalesQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(f => new RecentTransactionViewModel
                {
                    Id = f.Id,
                    Date = f.Date,
                    TransactionId = f.InvoiceNumber,
                    InvoiceNumber = f.InvoiceNumber,
                    PumpNumber = f.PumpNumber,
                    PaymentMethod = f.PaymentMethod.ToString(),
                    Liters = f.Liters,
                    Amount = f.Amount
                })
                .ToListAsync();

            var recentTransactions = allTransactions
                .OrderBy(t => {
                    var numericPart = new string(t.InvoiceNumber?.Where(char.IsDigit).ToArray());
                    if (int.TryParse(numericPart, out int num))
                        return num;
                    return 0;
                })
                .ThenBy(t => t.InvoiceNumber)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalCount = totalTransactions;

            // ========== DISTRIBUTION ITEMS ==========
            var distributionItems = new List<DistributionItemViewModel>();

            var topCustomers = await fuelSalesQuery
                .GroupBy(f => f.CustomerName)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    TotalAmount = g.Sum(f => f.Amount),
                    HasPending = g.Any(f => f.Status == FuelSaleStatus.Pending),
                    HasPaid = g.Any(f => f.Status == FuelSaleStatus.Paid),
                    HasCancelled = g.Any(f => f.Status == FuelSaleStatus.Cancelled)
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(5)
                .ToListAsync();

            int itemId = 1;
            foreach (var customer in topCustomers)
            {
                string status = "Paid";
                if (customer.HasPending)
                    status = "Pending";
                else if (customer.HasCancelled && !customer.HasPaid)
                    status = "Cancelled";

                string initials = "FC";
                var nameParts = customer.CustomerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                {
                    initials = (nameParts[0][0].ToString() + nameParts[1][0].ToString()).ToUpper();
                }
                else if (nameParts.Length == 1 && nameParts[0].Length >= 2)
                {
                    initials = nameParts[0].Substring(0, 2).ToUpper();
                }

                distributionItems.Add(new DistributionItemViewModel
                {
                    Id = itemId++,
                    ClientName = customer.CustomerName,
                    ClientId = $"SALE-{itemId:D3}",
                    Initials = initials,
                    Amount = customer.TotalAmount,
                    Status = status
                });
            }

            var viewModel = new DashboardViewModel
            {
                // Summary Cards - All users see these
                TotalRevenue = totalRevenue,
                TotalFuelSales = totalFuelSales,
                TotalLitersSold = totalLitersSold,
                ActivePumps = activePumps,
                TotalPumps = totalPumps,
                TodaysRevenue = todaysRevenue,
                AverageTransaction = avgTransaction,
                PendingInvoices = pendingInvoices,
                FuelLossVariance = fuelLossVariance,
                FleetUsage = fleetUsage,
                TotalInventory = totalInventoryStock,
                ActiveSessions = activeSessions,
                FuelingNow = fuelingNow,

                // Car Wash card data
                TodayCarWashes = todayCarWashes,
                TodayCarWashRevenue = todayCarWashRevenue,
                TotalCarWashes = totalCarWashes,
                TotalCarWashRevenue = totalCarWashRevenue,
                PendingCarWashes = pendingCarWashes,

                //  Fuel Clients card data
                TotalClients = totalClients,
                ActiveClients = activeClients,
                NewClientsThisMonth = newClientsThisMonth,

                // Compensation card data
                TotalCompensationPaidThisMonth = totalCompensationPaidThisMonth,
                TotalEmployees = totalEmployees,
                PendingCompensations = pendingCompensations,

                // Growth Percentages
                RevenueGrowthPercentage = revenueGrowth,
                LitersGrowthPercentage = litersGrowth,
                TodaysVsAveragePercentage = todaysVsAverage,

                // Chart Data
                MonthlySales = monthlySales,
                WeeklySales = weeklySales,

                // Recent Transactions
                RecentTransactions = recentTransactions,

                // Distribution Items
                DistributionItems = distributionItems,

                TotalTransactions = totalTransactions,
                TodaysTrend = todaysVsAverage > 0 ? "Upward trend" : "Downward trend",

                // Add role information to view model
                UserRole = userRole,
                IsAdmin = _authService.IsAdmin()
            };

            return View(viewModel);
        }

        // These actions should be restricted to Admins only
        [HttpGet]
        public IActionResult Compensation()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Only Admins can access Compensation
            if (!_authService.IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewData["Title"] = "Compensation";
            return View();
        }

        [HttpGet]
        public IActionResult Reports()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Only Admins can access Reports
            if (!_authService.IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewData["Title"] = "Reports";
            return View();
        }

        // These actions are accessible to all authenticated users
        [HttpGet]
        public IActionResult FuelSales()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Fuel Sales";
            return View();
        }

        [HttpGet]
        public IActionResult Inventory()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Inventory";
            return View();
        }

        [HttpGet]
        public IActionResult FleetMgmt()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Fleet Management";
            return View();
        }

        [HttpGet]
        public IActionResult Pumps()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Pumps";
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Settings";
            return View();
        }
    }
}