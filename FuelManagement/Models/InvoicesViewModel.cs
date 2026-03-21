using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class InvoicesViewModel
    {
        public InvoiceFilterViewModel Filter { get; set; }
        public List<InvoiceListItemViewModel> InvoiceItems { get; set; }

        // Changed from calculated properties to properties with setters
        // This allows the controller to assign values directly
        public int TotalInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalOverdue { get; set; }
        public decimal TotalPartial { get; set; }

        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public int PartialCount { get; set; }
        public int OverdueCount { get; set; }

        public decimal AverageInvoiceValue { get; set; }
        public decimal CollectionRate { get; set; }
        public decimal TotalVAT { get; set; }

        // ========== FIXED COMPACT FORMATTING WITH FLOOR ROUNDING ==========
        // Helper method to format revenue in compact form (for badge)
        public string GetFormattedRevenue()
        {
            if (TotalRevenue >= 1000000000000) // 1 Trillion and above
            {
                decimal trillions = Math.Floor(TotalRevenue / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (TotalRevenue >= 1000000000) // 1 Billion and above
            {
                decimal billions = Math.Floor(TotalRevenue / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (TotalRevenue >= 1000000) // 1 Million and above
            {
                decimal millions = Math.Floor(TotalRevenue / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (TotalRevenue >= 1000) // 1 Thousand and above
            {
                decimal thousands = Math.Floor(TotalRevenue / 100) / 10;
                return $"${thousands:F1}K";
            }
            else // Below 1 Thousand - FIXED to show decimals
            {
                return TotalRevenue.ToString("C2"); // Changed from "C0" to "C2"
            }
        }

        // Helper method to format TotalPaid in compact form (for main display)
        public string GetFormattedTotalPaid()
        {
            if (TotalPaid >= 1000000000000) // 1 Trillion and above
            {
                decimal trillions = Math.Floor(TotalPaid / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (TotalPaid >= 1000000000) // 1 Billion and above
            {
                decimal billions = Math.Floor(TotalPaid / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (TotalPaid >= 1000000) // 1 Million and above
            {
                decimal millions = Math.Floor(TotalPaid / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (TotalPaid >= 1000) // 1 Thousand and above
            {
                decimal thousands = Math.Floor(TotalPaid / 100) / 10;
                return $"${thousands:F1}K";
            }
            else // Below 1 Thousand - FIXED to show decimals
            {
                return TotalPaid.ToString("C2"); // Changed from "C0" to "C2"
            }
        }

        // Helper method to format TotalOutstanding in compact form (for main display)
        public string GetFormattedTotalOutstanding()
        {
            if (TotalOutstanding >= 1000000000000) // 1 Trillion and above
            {
                decimal trillions = Math.Floor(TotalOutstanding / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (TotalOutstanding >= 1000000000) // 1 Billion and above
            {
                decimal billions = Math.Floor(TotalOutstanding / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (TotalOutstanding >= 1000000) // 1 Million and above
            {
                decimal millions = Math.Floor(TotalOutstanding / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (TotalOutstanding >= 1000) // 1 Thousand and above
            {
                decimal thousands = Math.Floor(TotalOutstanding / 100) / 10;
                return $"${thousands:F1}K";
            }
            else // Below 1 Thousand - FIXED to show decimals
            {
                return TotalOutstanding.ToString("C2"); // Changed from "C0" to "C2"
            }
        }

        // Helper method to format TotalOverdue in compact form (for main display)
        public string GetFormattedTotalOverdue()
        {
            if (TotalOverdue >= 1000000000000) // 1 Trillion and above
            {
                decimal trillions = Math.Floor(TotalOverdue / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (TotalOverdue >= 1000000000) // 1 Billion and above
            {
                decimal billions = Math.Floor(TotalOverdue / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (TotalOverdue >= 1000000) // 1 Million and above
            {
                decimal millions = Math.Floor(TotalOverdue / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (TotalOverdue >= 1000) // 1 Thousand and above
            {
                decimal thousands = Math.Floor(TotalOverdue / 100) / 10;
                return $"${thousands:F1}K";
            }
            else // Below 1 Thousand - FIXED to show decimals
            {
                return TotalOverdue.ToString("C2"); // Changed from "C0" to "C2"
            }
        }

        // Helper method to format TotalPartial in compact form (for main display)
        public string GetFormattedTotalPartial()
        {
            if (TotalPartial >= 1000000000000) // 1 Trillion and above
            {
                decimal trillions = Math.Floor(TotalPartial / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (TotalPartial >= 1000000000) // 1 Billion and above
            {
                decimal billions = Math.Floor(TotalPartial / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (TotalPartial >= 1000000) // 1 Million and above
            {
                decimal millions = Math.Floor(TotalPartial / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (TotalPartial >= 1000) // 1 Thousand and above
            {
                decimal thousands = Math.Floor(TotalPartial / 100) / 10;
                return $"${thousands:F1}K";
            }
            else // Below 1 Thousand - FIXED to show decimals
            {
                return TotalPartial.ToString("C2"); // Changed from "C0" to "C2"
            }
        }

        // Helper method to format TotalVAT in compact form (for subtitle)
        public string GetFormattedTotalVAT()
        {
            if (TotalVAT >= 1000000000000) // 1 Trillion and above
            {
                decimal trillions = Math.Floor(TotalVAT / 100000000000) / 10;
                return $"${trillions:F1}T";
            }
            else if (TotalVAT >= 1000000000) // 1 Billion and above
            {
                decimal billions = Math.Floor(TotalVAT / 100000000) / 10;
                return $"${billions:F1}B";
            }
            else if (TotalVAT >= 1000000) // 1 Million and above
            {
                decimal millions = Math.Floor(TotalVAT / 100000) / 10;
                return $"${millions:F1}M";
            }
            else if (TotalVAT >= 1000) // 1 Thousand and above
            {
                decimal thousands = Math.Floor(TotalVAT / 100) / 10;
                return $"${thousands:F1}K";
            }
            else // Below 1 Thousand - FIXED to show decimals
            {
                return TotalVAT.ToString("C2"); // Changed from "C0" to "C2"
            }
        }
    }
}