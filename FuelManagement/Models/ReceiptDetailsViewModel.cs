using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class ReceiptDetailsViewModel
    {
        public int Id { get; set; }
        public string ReceiptNo { get; set; }
        public string InvoiceRef { get; set; }

        // Add this line to include the InvoiceId
        public int? InvoiceId { get; set; }

        // Customer Info
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }

        // Fuel Details
        public string FuelType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment Details
        public decimal AmountPaid { get; set; }
        public decimal BalanceRemaining { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }

        // Station Info
        public string StationName { get; set; }
        public string StationAddress { get; set; }
        public string StationPhone { get; set; }
        public string StationEmail { get; set; }

        // Metadata
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; }

        // Formatted Properties
        public string FormattedReceiptNo => ReceiptNo;
        public string FormattedDate => PaymentDate.ToString("MMMM dd, yyyy");
        public string FormattedTime => PaymentDate.ToString("hh:mm tt");
        public string FormattedDateTime => PaymentDate.ToString("MMM dd, yyyy hh:mm tt");
        public string FormattedQuantity => $"{Quantity:N1} L";
        public string FormattedUnitPrice => UnitPrice.ToString("C");
        public string FormattedSubtotal => Subtotal.ToString("C");
        public string FormattedVAT => VAT.ToString("C");
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedAmountPaid => AmountPaid.ToString("C");
        public string FormattedBalance => BalanceRemaining.ToString("C");

        public string StatusColor => PaymentStatus switch
        {
            "Paid" => "bg-emerald-100 text-emerald-800",
            "Pending" => "bg-amber-100 text-amber-800",
            "Partial" => "bg-purple-100 text-purple-800",
            "Overdue" => "bg-red-100 text-red-800",
            "Void" => "bg-gray-100 text-gray-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
}