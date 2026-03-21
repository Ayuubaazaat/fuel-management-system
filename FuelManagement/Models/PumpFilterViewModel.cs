using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class PumpFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PumpId { get; set; }
        public string? FuelType { get; set; }
        public string? TankId { get; set; }
        public string? Status { get; set; }

        public List<string> AvailableFuelTypes { get; set; } = new();
        public List<string> AvailableStatuses { get; set; } = new();
        public List<string> AvailablePumps { get; set; } = new();
        public List<string> AvailableTanks { get; set; } = new();

        public bool HasFilters =>
            StartDate.HasValue ||
            EndDate.HasValue ||
            !string.IsNullOrEmpty(PumpId) ||
            !string.IsNullOrEmpty(FuelType) ||
            !string.IsNullOrEmpty(TankId) ||
            !string.IsNullOrEmpty(Status);
    }
}