using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FuelManagement.Models
{
    public class TripEditViewModel
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Trip ID")]
        public string TripId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a vehicle")]
        [Display(Name = "Select Vehicle")]
        public string VehicleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle name is required")]
        [StringLength(100, ErrorMessage = "Vehicle name cannot exceed 100 characters")]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "License plate is required")]
        [StringLength(20, ErrorMessage = "License plate cannot exceed 20 characters")]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Driver name is required")]
        [StringLength(100, ErrorMessage = "Driver name cannot exceed 100 characters")]
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trip date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Trip Date")]
        public DateTime TripDate { get; set; }

        [Required(ErrorMessage = "Distance is required")]
        [Range(0.1, 9999, ErrorMessage = "Distance must be between 0.1 and 9,999 km")]
        [Display(Name = "Distance (km)")]
        public decimal Distance { get; set; }

        [Required(ErrorMessage = "Fuel used is required")]
        [Range(0.1, 9999, ErrorMessage = "Fuel used must be between 0.1 and 9,999 L")]
        [Display(Name = "Fuel Used (L)")]
        public decimal FuelUsed { get; set; }

        [Display(Name = "Fuel Efficiency (L/100km)")]
        public decimal FuelEfficiency { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0.01, 999999, ErrorMessage = "Cost must be between 0.01 and 999,999")]
        [Display(Name = "Cost ($)")]
        public decimal Cost { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Timestamp for concurrency
        public DateTime? UpdatedAt { get; set; }

        // For dropdown
        public List<SelectListItem> AvailableVehicles { get; set; } = new();
    }
}