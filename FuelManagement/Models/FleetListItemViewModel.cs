using System;

namespace FuelManagement.Models
{
    public class FleetListItemViewModel
    {
        public int Id { get; set; }
        public string VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string DriverName { get; set; }
        public string LicensePlate { get; set; }
        public string FuelType { get; set; }
        public int TotalTrips { get; set; }
        public decimal TotalFuelConsumed { get; set; }
        public decimal FuelEfficiency { get; set; } // Liters per 100km
        public string Status { get; set; }
        public DateTime LastServiceDate { get; set; }
        public int Odometer { get; set; }
        public DateTime NextServiceDue { get; set; }
        public DateTime CreatedAt { get; set; } // Added this property

        public string FormattedLastService => LastServiceDate.ToString("MMM dd, yyyy");
        public string FormattedNextService => NextServiceDue.ToString("MMM dd, yyyy");
        public string FormattedOdometer => $"{Odometer:N0} km";
        public string FormattedFuelConsumed => $"{TotalFuelConsumed:N1}L";
        public string FormattedFuelEfficiency => $"{FuelEfficiency:F1}L/100km";

        public string StatusColor => Status switch
        {
            "Active" => "bg-green-100 text-green-800 border-green-200",
            "Maintenance" => "bg-yellow-100 text-yellow-800 border-yellow-200",
            "Inactive" => "bg-gray-100 text-gray-800 border-gray-200",
            _ => "bg-gray-100 text-gray-800 border-gray-200"
        };

        public string ProgressBarColor => Status switch
        {
            "Active" => "bg-gradient-to-r from-green-400 to-emerald-500",
            "Maintenance" => "bg-gradient-to-r from-yellow-400 to-amber-500",
            "Inactive" => "bg-gradient-to-r from-gray-400 to-gray-500",
            _ => "bg-gradient-to-r from-gray-400 to-gray-500"
        };

        public int ServiceDuePercentage
        {
            get
            {
                var daysUntilDue = (NextServiceDue - DateTime.UtcNow.Date).Days;
                if (daysUntilDue <= 0) return 100;
                if (daysUntilDue >= 30) return 0;
                return 100 - (int)((daysUntilDue / 30.0) * 100);
            }
        }
    }
}