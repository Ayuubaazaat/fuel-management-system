using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class InventoryEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tank ID is required")]
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
        public DateTime LastRefillDate { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; }

        public decimal Percentage => Capacity > 0 ? (CurrentStock / Capacity) * 100 : 0;
    }
}