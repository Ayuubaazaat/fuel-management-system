using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class InventoryViewModel
    {
        public InventoryFilterViewModel Filter { get; set; }

        public List<InventoryListItemViewModel> InventoryItems { get; set; }
            = new List<InventoryListItemViewModel>();

        // Store all filtered items for summary calculations
        public List<InventoryListItemViewModel> AllFilteredItems { get; set; }
            = new List<InventoryListItemViewModel>();

        // Summary card data - These are now settable properties
        public decimal TotalFuelStock { get; set; }
        public int LowStockItems { get; set; }
        public decimal TotalCapacity { get; set; }
        public decimal InventoryValue { get; set; }
        public decimal AverageFillLevel { get; set; }

        // Dynamic percentage calculations
        public decimal FuelStockGrowthPercentage { get; set; }
        public decimal InventoryValueGrowthPercentage { get; set; }

        // Calculated property based on LowStockItems
        public string LowStockStatus => LowStockItems > 0 ? "Needs Attention" : "Good";
    }
}