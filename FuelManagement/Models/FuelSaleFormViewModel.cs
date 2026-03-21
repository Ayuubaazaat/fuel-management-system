using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public abstract class FuelSaleFormViewModel
    {
        [Required(ErrorMessage = "Invoice number is required")]
        [RegularExpression(@"^INV-\d+$",
                    ErrorMessage = "Invoice number must be in format: INV-001 (e.g., INV-001, INV-002)")]
        [Display(Name = "Invoice Number")]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        [StringLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Liters is required")]
        [Range(0.01, 1000000, ErrorMessage = "Liters must be greater than 0")]
        [Display(Name = "Liters")]
        public decimal Liters { get; set; }

        [Required(ErrorMessage = "Price per liter is required")]
        [Range(0.01, 1000, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price per Liter")]
        public decimal PricePerLiter { get; set; }

        // Calculated automatically — not stored in form
        [Display(Name = "Amount")]
        public decimal Amount => Liters * PricePerLiter;

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Pump number is required")]
        [Display(Name = "Pump Number")]
        [StringLength(20)]
        public string PumpNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}