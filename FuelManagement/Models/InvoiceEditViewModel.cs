using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FuelManagement.Models
{
    public class InvoiceEditViewModel
    {
        private const decimal VAT_RATE = 0.05m;

        [Required(ErrorMessage = "Invoice Number is required")]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Customer type is required")]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [Display(Name = "Customer Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Remote(action: "CheckCustomerPhoneForEdit", controller: "Invoices", AdditionalFields = "InvoiceNumber", ErrorMessage = "This Phone Number already exists")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Customer Email")]
        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [Display(Name = "Customer Address")]
        public string? CustomerAddress { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Required(ErrorMessage = "Total liters is required")]
        [Range(0.1, 1000000, ErrorMessage = "Total liters must be between 0.1 and 1,000,000")]
        [Display(Name = "Total Liters")]
        public decimal TotalLiters { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 100, ErrorMessage = "Unit price must be between 0.01 and 100")]
        [Display(Name = "Unit Price ($)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Amount Paid ($)")]
        [Range(0, 1000000, ErrorMessage = "Amount paid must be between 0 and 1,000,000")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Issue date is required")]
        [Display(Name = "Issue Date")]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        // Calculated fields
        public decimal Subtotal => TotalLiters * UnitPrice;
        public decimal VAT => Subtotal * VAT_RATE;
        public decimal TotalAmount => Subtotal + VAT;
        public decimal BalanceDue => TotalAmount - AmountPaid;
    }
}

