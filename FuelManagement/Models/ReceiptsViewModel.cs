using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class ReceiptsViewModel
    {
        public ReceiptFilterViewModel Filter { get; set; }
        public List<ReceiptListItemViewModel> ReceiptItems { get; set; }

        // Summary Card Properties
        public int TotalReceipts { get; set; }
        public decimal TotalAmountCollected { get; set; }
        public decimal CollectionRate { get; set; }

        // Previous month data for percentage calculations
        public int PreviousMonthTotalReceipts { get; set; }
        public decimal PreviousMonthTotalCollected { get; set; }

        // Counts
        public int PaidCount { get; set; }
        public int PendingCount { get; set; }
        public int PartialCount { get; set; }
        public int OverdueCount { get; set; }
        public int VoidCount { get; set; }

        // Amounts
        public decimal PendingAmount { get; set; }
        public decimal PartialAmount { get; set; }
        public decimal OverdueAmount { get; set; }

        // For the buttons
        public decimal TotalPartialPayments => PartialAmount;
        public decimal TotalOverduePayments => OverdueAmount;
        public decimal TotalPendingPayments => PendingAmount;
    }
}