using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class FleetMgmtViewModel
    {
        public FleetFilterViewModel Filter { get; set; }

        public List<FleetListItemViewModel> FleetItems { get; set; }
            = new List<FleetListItemViewModel>();

        // Store all filtered items for summary calculations
        public List<FleetListItemViewModel> AllFilteredItems { get; set; }
            = new List<FleetListItemViewModel>();

        // Summary card data
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int MaintenanceVehicles { get; set; }
        public int VehiclesDueForService { get; set; }
        public decimal TotalFuelConsumption { get; set; }
        public decimal AverageFuelEfficiency { get; set; }

        // New vehicles added in the last 30 days
        public int NewVehiclesThisMonth { get; set; }
    }
}