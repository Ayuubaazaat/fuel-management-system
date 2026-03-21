using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class FleetEditViewModel
    {
        [Required(ErrorMessage = "Vehicle ID is required")]
        [Display(Name = "Vehicle ID")]
        public string VehicleId { get; set; }

        [Required(ErrorMessage = "Vehicle name is required")]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; }

        [Required(ErrorMessage = "Driver name is required")]
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; }

        [Required(ErrorMessage = "License plate is required")]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Display(Name = "Total Fuel Consumed (L)")]
        public decimal TotalFuelConsumed { get; set; }

        [Display(Name = "Fuel Efficiency (L/100km)")]
        public decimal FuelEfficiency { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Last service date is required")]
        [Display(Name = "Last Service Date")]
        public DateTime LastServiceDate { get; set; }

        [Required(ErrorMessage = "Odometer reading is required")]
        [Range(0, 999999, ErrorMessage = "Odometer must be between 0 and 999,999")]
        [Display(Name = "Odometer (km)")]
        public int Odometer { get; set; }

        [Required(ErrorMessage = "Next service due date is required")]
        [Display(Name = "Next Service Due")]
        public DateTime NextServiceDue { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}