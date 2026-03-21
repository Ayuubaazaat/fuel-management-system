using System;

namespace FuelManagement.Models
{
    public class CompensationListItemViewModel
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        public string FormattedPaymentDate => PaymentDate.ToString("MMM dd, yyyy");
        public string FormattedBaseSalary => BaseSalary.ToString("C");
        public string FormattedBonus => Bonus.ToString("C");
        public string FormattedDeductions => Deductions.ToString("C");
        public string FormattedNetSalary => NetSalary.ToString("C");
        public int DaysUntilPayment => (PaymentDate - DateTime.Today).Days;

        public string StatusColor => Status switch
        {
            "Paid" => "bg-green-100 text-green-800 border-green-200",
            "Pending" => "bg-yellow-100 text-yellow-800 border-yellow-200",
            "Overdue" => "bg-red-100 text-red-800 border-red-200",
            _ => "bg-gray-100 text-gray-800 border-gray-200"
        };

        public string StatusIcon => Status switch
        {
            "Paid" => "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z",
            "Pending" => "M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z",
            "Overdue" => "M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z",
            _ => "M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
        };
    }
}