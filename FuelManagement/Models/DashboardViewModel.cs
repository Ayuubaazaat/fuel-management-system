using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class DashboardViewModel
    {
        // Summary Cards
        public decimal TotalRevenue { get; set; }
        public decimal TotalFuelSales { get; set; }
        public decimal TotalLitersSold { get; set; }
        public int ActivePumps { get; set; }
        public int TotalPumps { get; set; }
        public decimal TodaysRevenue { get; set; }
        public decimal AverageTransaction { get; set; }
        public int PendingInvoices { get; set; }
        public decimal FuelLossVariance { get; set; }
        public decimal FleetUsage { get; set; }
        public decimal TotalInventory { get; set; }
        public int ActiveSessions { get; set; }
        public int FuelingNow { get; set; }

        // Comparison Data
        public decimal RevenueGrowthPercentage { get; set; }
        public decimal LitersGrowthPercentage { get; set; }
        public decimal TodaysVsAveragePercentage { get; set; }

        // Chart Data
        public List<MonthlySalesData> MonthlySales { get; set; } = new();

        // Weekly Chart Data
        public List<WeeklySalesData> WeeklySales { get; set; } = new();

        // Recent Transactions
        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();

        // Distribution List (Vendor/Client Payments)
        public List<DistributionItemViewModel> DistributionItems { get; set; } = new();

        // Stats
        public int TotalTransactions { get; set; }
        public string TodaysTrend { get; set; } = "Steady trend";

        // ===== NEW: Role-based properties =====
        public string? UserRole { get; set; }
        public bool IsAdmin { get; set; }

        // Helper method to format Total Fuel Sales in compact form
        public string GetFormattedTotalFuelSales()
        {
            if (TotalFuelSales >= 1000000000000) // 1 Trillion and above
            {
                return $"${(TotalFuelSales / 1000000000000):F1}T";
            }
            else if (TotalFuelSales >= 1000000000) // 1 Billion and above
            {
                return $"${(TotalFuelSales / 1000000000):F1}B";
            }
            else if (TotalFuelSales >= 1000000) // 1 Million and above
            {
                return $"${(TotalFuelSales / 1000000):F1}M";
            }
            else if (TotalFuelSales >= 1000) // 1 Thousand and above
            {
                return $"${(TotalFuelSales / 1000):F1}K";
            }
            else // Below 1 Thousand
            {
                return $"${TotalFuelSales:F0}";
            }
        }

        // Helper method to format Total Liters Sold in compact form
        public string GetFormattedTotalLitersSold()
        {
            if (TotalLitersSold >= 1000000) // 1 Million liters and above
            {
                return $"{(TotalLitersSold / 1000000):F1}ML";
            }
            else if (TotalLitersSold >= 1000) // 1 Thousand liters and above
            {
                return $"{(TotalLitersSold / 1000):F1}KL";
            }
            else // Below 1 Thousand
            {
                return $"{TotalLitersSold:F0}L";
            }
        }

        // Helper method to format Today's Revenue in compact form (for badge)
        public string GetFormattedTodaysRevenue()
        {
            if (TodaysRevenue >= 1000000000000) // 1 Trillion and above
            {
                return $"${(TodaysRevenue / 1000000000000):F1}T";
            }
            else if (TodaysRevenue >= 1000000000) // 1 Billion and above
            {
                return $"${(TodaysRevenue / 1000000000):F1}B";
            }
            else if (TodaysRevenue >= 1000000) // 1 Million and above
            {
                return $"${(TodaysRevenue / 1000000):F1}M";
            }
            else if (TodaysRevenue >= 1000) // 1 Thousand and above
            {
                return $"${(TodaysRevenue / 1000):F1}K";
            }
            else // Below 1 Thousand
            {
                return $"${TodaysRevenue:F0}";
            }
        }

        // ===== NEW: Role-based helper methods =====
        public bool CanViewReports() => IsAdmin;
        public bool CanViewCompensation() => IsAdmin;
        public bool CanManageUsers() => IsAdmin;
        public string GetRoleBadgeClass() => IsAdmin ? "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400" : "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400";
        public string GetRoleIcon() => IsAdmin ? "fa-crown" : "fa-user";
    }
}