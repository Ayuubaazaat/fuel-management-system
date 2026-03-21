using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class FuelSaleViewModel
    {
        public FuelSaleFilterViewModel Filter { get; set; }
        public List<FuelSaleListItemViewModel> Sales { get; set; }

        // Summary card data
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalLitersSold { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TodaysRevenue { get; set; }
        public decimal AverageTransaction { get; set; }
        public int PendingInvoices { get; set; }

        // Comparison data
        public decimal LastMonthSalesAmount { get; set; }
        public decimal LastMonthLitersSold { get; set; }
        public decimal AverageDailyRevenue { get; set; }
        public int PreviousPendingInvoices { get; set; }

        // Calculated percentages
        public decimal SalesGrowthPercentage { get; set; }
        public decimal LitersGrowthPercentage { get; set; }
        public decimal RevenueVsAveragePercentage { get; set; }
        public decimal PendingChangePercentage { get; set; }

        // Pagination properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int StartRow => ((PageNumber - 1) * PageSize) + 1;
        public int EndRow => Math.Min(PageNumber * PageSize, TotalCount);

        // ========== FIXED COMPACT FORMATTING WITH FLOOR ROUNDING ==========
        // This handles: K (thousands), M (millions), B (billions), T (trillions)
        // Uses floor rounding to show 1.9B instead of 2.0B for 1,997,998,002
        public string GetCompactRevenue(decimal revenue)
        {
            if (revenue >= 1000000000000) // 1 Trillion and above
            {
                // Use Math.Floor to truncate instead of round
                decimal trillions = Math.Floor(revenue / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (revenue >= 1000000000) // 1 Billion and above
            {
                // Use Math.Floor to truncate instead of round
                decimal billions = Math.Floor(revenue / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (revenue >= 1000000) // 1 Million and above
            {
                // Use Math.Floor to truncate instead of round
                decimal millions = Math.Floor(revenue / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (revenue >= 1000) // 1 Thousand and above
            {
                // Use Math.Floor to truncate instead of round
                decimal thousands = Math.Floor(revenue / 100) / 10;
                return $"${thousands:F1}K";
            }
            else // Below 1 Thousand
            {
                return revenue.ToString("C0");
            }
        }

        // Also fix the liters formatting
        public string GetFormattedTotalLitersSold()
        {
            if (TotalLitersSold >= 1000000) // 1 Million liters and above
            {
                decimal millions = Math.Floor(TotalLitersSold / 100000) / 10;
                return $"{millions:F1}M L";
            }
            else if (TotalLitersSold >= 1000) // 1 Thousand liters and above
            {
                decimal thousands = Math.Floor(TotalLitersSold / 100) / 10;
                return $"{thousands:F1}K L";
            }
            else // Below 1 Thousand
            {
                return $"{TotalLitersSold:F0} L";
            }
        }
    }
}