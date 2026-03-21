using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class FleetFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string VehicleId { get; set; }
        public string FuelType { get; set; }
        public string Status { get; set; }

        public List<string> AvailableVehicles => new List<string> { "VH-001", "VH-002", "VH-003", "VH-004", "VH-005" };
        public List<string> AvailableFuelTypes => new List<string> { "Diesel", "Premium Gasoline", "Regular Gasoline", "Electric" };
        public List<string> AvailableStatuses => new List<string> { "Active", "Maintenance", "Inactive" };
    }
}