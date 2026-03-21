using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FuelManagement.Models
{
    public class PumpCreateViewModel
    {
        [Required(ErrorMessage = "Pump ID is required")]

        [StringLength(10, MinimumLength = 7, ErrorMessage = "Pump ID must be between 7 and 10 characters")]
        [RegularExpression(@"^PMP-\d{3}$", ErrorMessage = "Pump ID must be in format PMP-001")]
        [Remote(action: "CheckPumpId", controller: "Pumps", ErrorMessage = "Pump ID already exists")]
        [Display(Name = "Pump ID")]
        public string PumpId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pump name is required")]
        [StringLength(100, ErrorMessage = "Pump name cannot exceed 100 characters")]
        [Display(Name = "Pump Name/Description")]
        public string PumpName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tank selection is required")]
        [Display(Name = "Tank")]
        public string TankId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Flow rate is required")]
        [Range(0.1, 200, ErrorMessage = "Flow rate must be between 0.1 and 200 L/min")]
        [Display(Name = "Flow Rate (L/min)")]
        public decimal FlowRate { get; set; } = 45.0m;

        [Required(ErrorMessage = "Installation date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Installation Date")]
        public DateTime InstallationDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Last Maintenance Date")]
        public DateTime? LastMaintenanceDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Next Maintenance Due")]
        public DateTime? NextMaintenanceDue { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        // For dropdowns - will be populated in controller
        public List<SelectListItem>? FuelTypes { get; set; }
        public List<SelectListItem>? Tanks { get; set; }
        public List<SelectListItem>? StatusOptions { get; set; }
    }
}