namespace FuelManagement.Models
{
    public class ReportSummaryViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalVATCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal CollectionRate { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int OverdueInvoices { get; set; }

        public string FormattedTotalRevenue => FormatCurrency(TotalRevenue);
        public string FormattedTotalVAT => FormatCurrency(TotalVATCollected);
        public string FormattedTotalOutstanding => FormatCurrency(TotalOutstanding);
        public string FormattedCollectionRate => $"{CollectionRate:F1}%";

        public string CollectionRateColor => CollectionRate >= 85 ? "text-green-600 dark:text-green-400" :
                                              CollectionRate >= 70 ? "text-yellow-600 dark:text-yellow-400" :
                                              "text-red-600 dark:text-red-400";

        // FIXED: Helper method to format currency with K, M, B, T suffixes using floor rounding
        private string FormatCurrency(decimal amount)
        {
            if (amount >= 1000000000000) // 1 Trillion and above
            {
                decimal trillions = Math.Floor(amount / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (amount >= 1000000000) // 1 Billion and above
            {
                decimal billions = Math.Floor(amount / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (amount >= 1000000) // 1 Million and above
            {
                decimal millions = Math.Floor(amount / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (amount >= 1000) // 1 Thousand and above
            {
                decimal thousands = Math.Floor(amount / 100) / 10;
                return $"${thousands:F1}K";
            }
            else
            {
                return amount.ToString("C0");
            }
        }
    }
}