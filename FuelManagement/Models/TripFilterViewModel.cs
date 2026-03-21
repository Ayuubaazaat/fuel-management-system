using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class TripFilterViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Trip ID")]  // Changed from Vehicle to Trip ID
        public string? TripId { get; set; }

        [Display(Name = "Driver Name")]
        public string? DriverName { get; set; }

        [Display(Name = "Min Distance")]
        public decimal? MinDistance { get; set; }

        [Display(Name = "Max Distance")]
        public decimal? MaxDistance { get; set; }

        [Display(Name = "Min Fuel Used")]
        public decimal? MinFuelUsed { get; set; }

        [Display(Name = "Max Fuel Used")]
        public decimal? MaxFuelUsed { get; set; }

        // Available options for dropdowns
        public List<string> AvailableVehicles { get; set; } = new();
        public List<string> AvailableDrivers { get; set; } = new();

        // Helper to check if any filter is applied
        public bool HasFilters =>
            StartDate.HasValue ||
            EndDate.HasValue ||
            !string.IsNullOrEmpty(TripId) ||  // Changed from VehicleId to TripId
            !string.IsNullOrEmpty(DriverName) ||
            MinDistance.HasValue ||
            MaxDistance.HasValue ||
            MinFuelUsed.HasValue ||
            MaxFuelUsed.HasValue;
    }
}