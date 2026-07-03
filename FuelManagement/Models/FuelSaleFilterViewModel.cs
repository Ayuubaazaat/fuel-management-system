using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class FuelSaleFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PumpNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }

        // Updated with your actual payment methods
        public List<string> AvailablePumps => new List<string> { "P-01", "P-02", "P-03", "P-04", "P-05", "P-06" };

        // FIXED: Your actual payment methods from the system
        public List<string> AvailablePaymentMethods => new List<string> { "EvcPlus", "JEEB", "EDahab" };

        public List<string> AvailableStatuses => new List<string> { "Paid", "Pending", "Cancelled" };
    }
}