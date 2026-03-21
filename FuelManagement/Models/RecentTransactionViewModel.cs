using System;

namespace FuelManagement.Models
{
    public class RecentTransactionViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Add this property

        // You can keep Date if needed elsewhere, but we won't display it

        public string TransactionId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PumpNumber { get; set; } = string.Empty;
        public decimal Liters { get; set; }
        public decimal Amount { get; set; }
        public string FormattedLiters => $"{Liters:N0} L";
        public string FormattedAmount => Amount.ToString("C0");
    }
}