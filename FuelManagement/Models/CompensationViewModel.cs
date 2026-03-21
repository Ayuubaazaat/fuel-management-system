using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class CompensationViewModel
    {
        public CompensationFilterViewModel Filter { get; set; }
        public List<CompensationListItemViewModel> CompensationItems { get; set; }

        // Change these from calculated properties to properties with setters
        // This allows the controller to assign values directly
        public decimal TotalPayroll { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public decimal TotalOverdue { get; set; }
        public int PaidCount { get; set; }
        public int PendingCount { get; set; }
        public int OverdueCount { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal TotalBonuses { get; set; }
        public decimal TotalDeductions { get; set; }
    }
}