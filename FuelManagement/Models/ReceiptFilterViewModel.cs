using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class ReceiptFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string ReceiptNo { get; set; }
        public string CustomerName { get; set; }

        public string CustomerType { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string FuelType { get; set; }

        public List<string> AvailableCustomerTypes => new List<string>
        {
            "Fleet",
            "Corporate",
            "Walk-in"
        };

        public List<string> AvailablePaymentStatuses => new List<string>
        {
            "Paid",
            "Pending",
            "Partial",
            "Overdue",
            "Void"
        };

        // Updated to Somali Mobile Money only
        public List<string> AvailablePaymentMethods => new List<string>
        {
           "EVC+",
           "JEEB",
           "E-Dahab"
        };

        public List<string> AvailableFuelTypes => new List<string>
        {
            "Premium Gasoline",
            "Regular Gasoline",
            "Diesel",
            "Premium Diesel",
            "Ethanol"
        };
    }
}