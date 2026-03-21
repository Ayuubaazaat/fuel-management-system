using System;

namespace FuelManagement.Models
{
    public class InventoryListItemViewModel
    {
        // REMOVED: public int Id { get; set; }  // No longer needed

        public string TankId { get; set; }
        public string FuelType { get; set; }
        public decimal Capacity { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal Percentage { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Status { get; set; }

        public string FormattedLastUpdated => LastUpdated.ToString("MMM dd, yyyy");
        public string FormattedCapacity => $"{Capacity:N0}L";
        public string FormattedCurrentStock => $"{CurrentStock:N0}L";
        public string FormattedPercentage => $"{Percentage:F1}%";

        public string StatusColor => Status switch
        {
            "Available" => "bg-green-100 text-green-800 border-green-200",
            "Low" => "bg-yellow-100 text-yellow-800 border-yellow-200",
            "Empty" => "bg-red-100 text-red-800 border-red-200",
            "Critical" => "bg-red-100 text-red-800 border-red-200", // Added Critical
            _ => "bg-gray-100 text-gray-800 border-gray-200"
        };

        public string ProgressBarColor => Status switch
        {
            "Available" => "bg-gradient-to-r from-green-500 to-emerald-500",
            "Low" => "bg-gradient-to-r from-yellow-500 to-amber-500",
            "Empty" => "bg-gradient-to-r from-red-500 to-rose-500",
            "Critical" => "bg-gradient-to-r from-red-500 to-rose-500", // Added Critical
            _ => "bg-gradient-to-r from-gray-500 to-gray-600"
        };
    }
}