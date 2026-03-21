using System;

namespace FuelManagement.Models
{
    public class DistributionItemViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;

        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Paid" => "text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-900/30",      // Paid = Green
                    "Pending" => "text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-900/30",          // Pending = Amber/Yellow
                    "Cancelled" => "text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/30",                // Cancelled = Red
                    _ => "text-gray-600 dark:text-gray-400 bg-gray-50 dark:bg-gray-700"
                };
            }
        }

        public string InitialsColor
        {
            get
            {
                return Status switch
                {
                    "Paid" => "from-purple-100 to-purple-200 dark:from-purple-900 dark:to-purple-800 text-purple-700 dark:text-purple-300",
                    "Pending" => "from-amber-100 to-amber-200 dark:from-amber-900 dark:to-amber-800 text-amber-700 dark:text-amber-300",
                    "Cancelled" => "from-red-100 to-red-200 dark:from-red-900 dark:to-red-800 text-red-700 dark:text-red-300",
                    _ => "from-gray-100 to-gray-200 dark:from-gray-700 dark:to-gray-600 text-gray-700 dark:text-gray-300"
                };
            }
        }
    }
}