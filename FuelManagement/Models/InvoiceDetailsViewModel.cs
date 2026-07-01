using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class InvoiceDetailsViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }

        // Customer Info
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }

        // Invoice Details
        public string FuelType { get; set; }
        public decimal TotalLiters { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        // Company Info
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }

        // Metadata
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; }

        // Related Receipts
        public List<ReceiptListItemViewModel> RelatedReceipts { get; set; } = new List<ReceiptListItemViewModel>();

        // Formatted Properties
        public string FormattedInvoiceNumber => InvoiceNumber;
        public string FormattedIssueDate => IssueDate.ToString("MMMM dd, yyyy");
        public string FormattedDueDate => DueDate.ToString("MMMM dd, yyyy");
        public string FormattedCreatedAt => CreatedAt.ToString("MMMM dd, yyyy");
        public string FormattedTotalLiters => $"{TotalLiters:N0} L";
        public string FormattedUnitPrice => UnitPrice.ToString("C");
        public string FormattedSubtotal => Subtotal.ToString("C");
        public string FormattedVAT => VAT.ToString("C");
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedAmountPaid => AmountPaid.ToString("C");
        public string FormattedBalanceDue => BalanceDue.ToString("C");

        public int DaysUntilDue => (DueDate - DateTime.UtcNow.Date).Days;
        public bool IsOverdue => DaysUntilDue < 0 && BalanceDue > 0;

        public string StatusColor => PaymentStatus switch
        {
            "Paid" => "bg-emerald-100 text-emerald-800",
            "Unpaid" => "bg-gray-100 text-gray-800",
            "Partial" => "bg-purple-100 text-purple-800",
            "Overdue" => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
}