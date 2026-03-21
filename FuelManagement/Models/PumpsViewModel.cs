using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class PumpsViewModel
    {
        public PumpFilterViewModel Filter { get; set; } = new();
        public List<PumpListItemViewModel> PumpItems { get; set; } = new();

        // Pagination properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        // Summary card data - FIXED: TotalPumps is now settable
        public int TotalPumps { get; set; }
        public int ActivePumps { get; set; }
        public int MaintenancePumps { get; set; }
        public int InactivePumps { get; set; }
        public decimal TotalFuelDispensed { get; set; }

        public decimal AverageFlowRate { get; set; }

        public int TotalTransactions { get; set; }
        public int PumpsDueForMaintenance { get; set; }
        public int OverdueMaintenance { get; set; }

        // Formatted values for display
        public string FormattedTotalFuelDispensed => $"{TotalFuelDispensed:N0}L";
        public string FormattedAverageFlowRate => $"{AverageFlowRate:F1} L/min";
        public string FormattedTotalTransactions => TotalTransactions.ToString("N0");

        // Pagination helpers
        public int StartIndex => (PageNumber - 1) * PageSize + 1;
        public int EndIndex => Math.Min(PageNumber * PageSize, TotalItems);
    }
}