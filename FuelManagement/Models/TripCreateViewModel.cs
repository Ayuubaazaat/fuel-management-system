
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FuelManagement.Models
{
    public class TripCreateViewModel
    {
        [Required(ErrorMessage = "Trip ID is required")]
        [RegularExpression(@"^TRP-\d+$",
            ErrorMessage = "Trip ID must be in format: TRP-001 (e.g., TRP-015, TRP-016)")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Trip ID must be between 3 and 20 characters")]
        [Remote(action: "CheckTripId", controller: "Trips", ErrorMessage = "Trip ID already exists")]
        [Display(Name = "Trip ID")]
        public string TripId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a vehicle")]
        [Display(Name = "Select Vehicle")]
        public string VehicleId { get; set; } = string.Empty;  // This will store the Fleet VehicleId

        // These will be auto-filled when vehicle is selected
        public string VehicleName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trip date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Trip Date")]
        public DateTime TripDate { get; set; } = DateTime.UtcNow.Date;

        [Required(ErrorMessage = "Distance is required")]
        [Range(0.1, 9999, ErrorMessage = "Distance must be between 0.1 and 9,999 km")]
        [Display(Name = "Distance (km)")]
        public decimal Distance { get; set; }

        [Required(ErrorMessage = "Fuel used is required")]
        [Range(0.1, 9999, ErrorMessage = "Fuel used must be between 0.1 and 9,999 L")]
        [Display(Name = "Fuel Used (L)")]
        public decimal FuelUsed { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0.01, 999999, ErrorMessage = "Cost must be between 0.01 and 999,999")]
        [Display(Name = "Cost ($)")]
        public decimal Cost { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // For dropdown
        public List<SelectListItem> AvailableVehicles { get; set; } = new();
    }
}