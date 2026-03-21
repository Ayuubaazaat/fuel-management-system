using System;

namespace FuelManagement.Models
{
    public class ReportListItemViewModel
    {
        public int Id { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public decimal VAT { get; set; }
        public decimal Outstanding { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public string Status { get; set; }

        public string FormattedMonth => $"{Month} {Year}";
        public string FormattedRevenue => FormatCurrency(Revenue);
        public string FormattedVAT => FormatCurrency(VAT);
        public string FormattedOutstanding => FormatCurrency(Outstanding);
        public string FormattedTotalInvoices => TotalInvoices.ToString("N0");
        public string FormattedPaidInvoices => PaidInvoices.ToString("N0");
        public decimal PaidPercentage => TotalInvoices > 0 ? (PaidInvoices / (decimal)TotalInvoices) * 100 : 0;

        public string StatusColor => Status switch
        {
            "Healthy" => "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400 border-green-200 dark:border-green-800",
            "Warning" => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400 border-yellow-200 dark:border-yellow-800",
            "Critical" => "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400 border-red-200 dark:border-red-800",
            _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300 border-gray-200 dark:border-gray-600"
        };

        public int PeriodSortOrder
        {
            get
            {
                var monthIndex = Array.IndexOf(new[] {
                    "January", "February", "March", "April", "May", "June",
                    "July", "August", "September", "October", "November", "December"
                }, Month);

                if (monthIndex < 0) monthIndex = 0;
                return (Year * 100) + monthIndex + 1;
            }
        }

        // Helper method to format currency with K, M suffixes
        private string FormatCurrency(decimal amount)
        {
            if (amount >= 1000000) // 1M and above
            {
                return $"${(amount / 1000000).ToString("0.#")}M";
            }
            else if (amount >= 1000) // 1K and above
            {
                return $"${(amount / 1000).ToString("0.#")}K";
            }
            else
            {
                return amount.ToString("C");
            }
        }
    }
}