using System;

namespace FuelManagement.Models
{
    public class ReceiptListItemViewModel
    {
        public int Id { get; set; }
        public string ReceiptNo { get; set; }
        public string InvoiceRef { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string FuelType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FormattedReceiptNo => ReceiptNo;
        public string FormattedQuantity => $"{Quantity:N1}L";
        public string FormattedUnitPrice => UnitPrice.ToString("C");
        public string FormattedSubtotal => Subtotal.ToString("C");
        public string FormattedVAT => VAT.ToString("C");
        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedPaymentDate => PaymentDate.ToString("MMM dd, yyyy hh:mm tt");
        public string FormattedCreatedAt => CreatedAt.ToString("MMM dd, yyyy hh:mm tt");

        public string StatusColor => PaymentStatus switch
        {
            "Paid" => "bg-emerald-100 text-emerald-800 border-emerald-200",
            "Pending" => "bg-amber-100 text-amber-800 border-amber-200",
            "Partial" => "bg-purple-100 text-purple-800 border-purple-200",
            "Overdue" => "bg-red-100 text-red-800 border-red-200",
            "Void" => "bg-gray-100 text-gray-800 border-gray-200",
            _ => "bg-gray-100 text-gray-800 border-gray-200"
        };

        public string StatusIcon => PaymentStatus switch
        {
            "Paid" => "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z",
            "Pending" => "M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z",
            "Partial" => "M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z",
            "Overdue" => "M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z",
            "Void" => "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636",
            _ => "M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
        };
    }
}