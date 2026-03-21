using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class InventoryFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TankId { get; set; }
        public string FuelType { get; set; }
        public string Status { get; set; }

        public List<string> AvailableTanks => new List<string> { "TNK-001", "TNK-002", "TNK-003", "TNK-004", "TNK-005", "TNK-006" };
        public List<string> AvailableFuelTypes => new List<string> { "Premium Gasoline", "Regular Gasoline", "Diesel", "Premium Diesel", "Ethanol" };
        public List<string> AvailableStatuses => new List<string> { "Available", "Low", "Critical" };
    }
}