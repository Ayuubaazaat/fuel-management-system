using System;

namespace FuelManagement.Models
{
    public class TripListItemViewModel
    {
        public string TripId { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public decimal Distance { get; set; }
        public decimal FuelUsed { get; set; }
        public decimal FuelEfficiency { get; set; }
        public decimal Cost { get; set; }
        public string? Notes { get; set; }

        // Formatted values for display
        public string FormattedDate => TripDate.ToString("MMM dd, yyyy");
        public string FormattedDistance => $"{Distance:N0} km";
        public string FormattedFuelUsed => $"{FuelUsed:N1} L";
        public string FormattedEfficiency => $"{FuelEfficiency:F1} L/100km";
        public string FormattedCost => Cost.ToString("C");

        // Status color for efficiency (optional)
        public string EfficiencyColor => FuelEfficiency switch
        {
            <= 6 => "text-green-600 dark:text-green-400",
            <= 8 => "text-blue-600 dark:text-blue-400",
            <= 10 => "text-amber-600 dark:text-amber-400",
            _ => "text-red-600 dark:text-red-400"
        };

        // Progress bar for efficiency (optional)
        public int EfficiencyPercentage
        {
            get
            {
                if (FuelEfficiency <= 0) return 0;
                if (FuelEfficiency >= 20) return 100;
                return (int)((FuelEfficiency / 20) * 100);
            }
        }

        public string ProgressBarColor => FuelEfficiency switch
        {
            <= 6 => "bg-gradient-to-r from-green-400 to-green-500",
            <= 8 => "bg-gradient-to-r from-blue-400 to-blue-500",
            <= 10 => "bg-gradient-to-r from-amber-400 to-amber-500",
            _ => "bg-gradient-to-r from-red-400 to-red-500"
        };
    }
}