using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FuelManagement.Models
{
    public class ReceiptEditViewModel : IValidatableObject
    {
        private const decimal VAT_RATE = 0.05m;

        public int Id { get; set; }

        [Display(Name = "Receipt Number")]
        public string ReceiptNo { get; set; }

        [Display(Name = "Select Invoice")]
        public int? SelectedInvoiceId { get; set; }

        [ValidateNever]
        public List<SelectListItem> Invoices { get; set; }

        [Required(ErrorMessage = "Invoice reference is required")]
        [Display(Name = "Invoice Reference")]
        [Remote(action: "CheckInvoiceReference", controller: "Receipts", AdditionalFields = "Id", ErrorMessage = "Invoice reference already exists")]
        public string InvoiceRef { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Customer type is required")]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [Display(Name = "Customer Phone")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Remote(action: "CheckCustomerPhone", controller: "Receipts", AdditionalFields = "Id", ErrorMessage = "Phone number already exists")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Customer Email")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.1, 100000, ErrorMessage = "Quantity must be between 0.1 and 100,000 liters")]
        [Display(Name = "Quantity (Liters)")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 100, ErrorMessage = "Unit price must be between 0.01 and 100")]
        [Display(Name = "Unit Price ($)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Payment Method")]
        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Amount Paid")]
        [Range(0, 1000000, ErrorMessage = "Amount paid must be between 0 and 1,000,000")]
        public decimal? AmountPaid { get; set; }

        [Display(Name = "Payment Date")]
        [Required(ErrorMessage = "Payment date is required")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Invoice relationship
        public int? InvoiceId { get; set; }

        // Calculated fields
        public decimal Subtotal => Quantity * UnitPrice;
        public decimal VAT => Subtotal * VAT_RATE;
        public decimal TotalAmount => Subtotal + VAT;
        public decimal Balance => TotalAmount - (AmountPaid ?? 0);

        // Cross-field validation: Amount Paid cannot exceed Total Amount
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AmountPaid.HasValue && AmountPaid.Value > TotalAmount)
            {
                yield return new ValidationResult(
                    $"Amount paid (${AmountPaid.Value:N2}) cannot exceed the total amount (${TotalAmount:N2}).",
                    new[] { nameof(AmountPaid) });
            }
        }
    }
}