using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FuelManagement.Models
{
    public class FleetCreateViewModel
    {
        [Required(ErrorMessage = "Vehicle ID is required")]
        [RegularExpression(@"^VH-\d+$",
                   ErrorMessage = "Vehicle ID must be in format: VH-001 (e.g., VH-001, VH-002)")]
        [Display(Name = "Vehicle ID")]
        [Remote("CheckVehicleId", "FleetMgmt", ErrorMessage = "Vehicle ID already exists")]
        public string VehicleId { get; set; }

        [Required(ErrorMessage = "Vehicle name is required")]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; }

        [Required(ErrorMessage = "Driver name is required")]
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "License plate is required")]
        [Display(Name = "License Plate")]
        [Remote("CheckLicensePlate", "FleetMgmt", ErrorMessage = "License plate already exists")]
        public string LicensePlate { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Display(Name = "Total Fuel Consumed (L)")]
        public decimal TotalFuelConsumed { get; set; } = 0;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Last service date is required")]
        [Display(Name = "Last Service Date")]
        public DateTime LastServiceDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Odometer reading is required")]
        [Range(0, 999999, ErrorMessage = "Odometer must be between 0 and 999,999")]
        [Display(Name = "Odometer (km)")]
        public int Odometer { get; set; }

        [Required(ErrorMessage = "Next service due date is required")]
        [Display(Name = "Next Service Due")]
        [DataType(DataType.Date)]
        public DateTime NextServiceDue { get; set; } = DateTime.Today.AddMonths(1);

        [Display(Name = "Notes")]
        public string? Notes { get; set; }  // Made nullable with ?
    }
}