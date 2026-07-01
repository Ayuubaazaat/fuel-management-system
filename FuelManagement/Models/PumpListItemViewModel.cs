using System;

namespace FuelManagement.Models
{
    public class PumpListItemViewModel
    {
        public string PumpId { get; set; } = string.Empty;
        public string PumpName { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public string TankId { get; set; } = string.Empty;
        public decimal TotalFuelDispensed { get; set; }
        public decimal FlowRate { get; set; }
        public int TransactionCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDue { get; set; }

        // Formatted values for display
        public string FormattedTotalDispensed => $"{TotalFuelDispensed:N0}L";
        public string FormattedFlowRate => $"{FlowRate:F1} L/min";
        public string FormattedTransactions => TransactionCount.ToString("N0");
        public string FormattedLastMaintenance => LastMaintenanceDate?.ToString("MMM dd, yyyy") ?? "Never";
        public string FormattedNextMaintenance => NextMaintenanceDue?.ToString("MMM dd, yyyy") ?? "Not scheduled";

        // Status color classes for Tailwind CSS
        public string StatusColorClass => Status?.ToLower() switch
        {
            "active" => "bg-green-100 text-green-800 border-green-200",
            "maintenance" => "bg-yellow-100 text-yellow-800 border-yellow-200",
            "inactive" => "bg-gray-100 text-gray-800 border-gray-200",
            _ => "bg-gray-100 text-gray-800 border-gray-200"
        };

        // Progress bar color
        public string ProgressBarColor => Status?.ToLower() switch
        {
            "active" => "bg-gradient-to-r from-yellow-400 to-amber-500",
            "maintenance" => "bg-gradient-to-r from-orange-400 to-orange-500",
            "inactive" => "bg-gradient-to-r from-gray-400 to-gray-500",
            _ => "bg-gradient-to-r from-gray-400 to-gray-500"
        };

        // Maintenance progress calculation
        public int DaysUntilMaintenance
        {
            get
            {
                if (!NextMaintenanceDue.HasValue) return 0;
                return (NextMaintenanceDue.Value - DateTime.UtcNow.Date).Days;
            }
        }

        public int MaintenancePercentage
        {
            get
            {
                if (!NextMaintenanceDue.HasValue || !LastMaintenanceDate.HasValue) return 0;

                var daysUntilDue = DaysUntilMaintenance;
                if (daysUntilDue <= 0) return 100;

                var daysSinceLastMaintenance = (DateTime.UtcNow.Date - LastMaintenanceDate.Value).Days;
                var totalCycleDays = 90;

                if (NextMaintenanceDue.HasValue && LastMaintenanceDate.HasValue)
                {
                    totalCycleDays = (NextMaintenanceDue.Value - LastMaintenanceDate.Value).Days;
                }

                if (totalCycleDays <= 0) return 0;

                var percentage = (int)((double)daysSinceLastMaintenance / totalCycleDays * 100);
                return Math.Min(100, Math.Max(0, percentage));
            }
        }

        public bool IsDueSoon => DaysUntilMaintenance > 0 && DaysUntilMaintenance <= 7;
        public bool IsOverdue => DaysUntilMaintenance < 0;
    }
}