using System;

namespace FuelManagement.Models
{
    public class FuelSaleListItemViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal Liters { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string PumpNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }

        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        public string FormattedAmount => Amount.ToString("C");
        public string FormattedLiters => $"{Liters:N0}L";
        public string StatusColor => Status switch
        {
            "Paid" => "bg-green-100 text-green-800 border-green-200",
            "Pending" => "bg-yellow-100 text-yellow-800 border-yellow-200",
            "Cancelled" => "bg-red-100 text-red-800 border-red-200",
            _ => "bg-gray-100 text-gray-800 border-gray-200"
        };
    }
}