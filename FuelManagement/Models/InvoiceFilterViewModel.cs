using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class InvoiceFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string PaymentStatus { get; set; }
        public string FuelType { get; set; }

        // Changed from read-only properties to properties with setters
        public List<string> AvailableCustomerTypes { get; set; } = new List<string>();
        public List<string> AvailablePaymentStatuses { get; set; } = new List<string>();
        public List<string> AvailableFuelTypes { get; set; } = new List<string>();

        // Static lists for default values
        public static readonly List<string> DefaultPaymentStatuses = new List<string>
        {
            "Paid",
            "Unpaid",
            "Partial",
            "Overdue"
        };

        public static readonly List<string> DefaultFuelTypes = new List<string>
        {
            "Premium Gasoline",
            "Regular Gasoline",
            "Diesel",
            "Premium Diesel",
            "Ethanol"
        };

        public static readonly List<string> DefaultCustomerTypes = new List<string>
        {
            "Fleet",
            "Corporate",
            "Walk-in"
        };
    }
}