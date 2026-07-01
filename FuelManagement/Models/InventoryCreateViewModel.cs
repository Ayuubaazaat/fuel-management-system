using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class InventoryCreateViewModel
    {
        [Required(ErrorMessage = "Tank ID is required")]
        [RegularExpression(@"^TANK-\d+$",
            ErrorMessage = "Tank ID must be in format: TANK-001 (only TANK prefix allowed)")]
        [Display(Name = "Tank ID")]
        public string TankId { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 1000000, ErrorMessage = "Capacity must be greater than 0")]
        [Display(Name = "Capacity (Liters)")]
        public decimal Capacity { get; set; }

        [Required(ErrorMessage = "Current stock is required")]
        [Range(0, 1000000, ErrorMessage = "Current stock must be between 0 and capacity")]
        [Display(Name = "Current Stock (Liters)")]
        public decimal CurrentStock { get; set; }

        [Display(Name = "Last Refill Date")]
        public DateTime LastRefillDate { get; set; } = DateTime.UtcNow.Date;

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Available";
    }
}