using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FuelManagement.Models
{
    public class PumpEditViewModel
    {
        [Required]
        [StringLength(10)]
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
        public decimal FlowRate { get; set; }

        [Display(Name = "Total Fuel Dispensed (L)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalFuelDispensed { get; set; }

        [Display(Name = "Transaction Count")]
        public int TransactionCount { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Installation Date")]
        public DateTime InstallationDate { get; set; }

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
        public string Status { get; set; } = string.Empty;

        // For dropdowns
        public List<SelectListItem>? FuelTypes { get; set; }
        public List<SelectListItem>? Tanks { get; set; }
        public List<SelectListItem>? StatusOptions { get; set; }

        // Timestamp for concurrency
        public DateTime? UpdatedAt { get; set; }
    }
}