using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class ReportFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportType { get; set; }

        public List<string> AvailableReportTypes => new List<string>
        {
            "Revenue by Month",
            "VAT Collected",
            "Outstanding Balances",
            "Payment Status",
            "Customer Summary",
            "Fuel Type Analysis"
        };
    }
}