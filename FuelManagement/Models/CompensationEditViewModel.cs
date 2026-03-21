using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class CompensationEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }

        // Phone number is stored but NOT editable in edit mode
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [Display(Name = "Position")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Base salary is required")]
        [Range(0, 1000000, ErrorMessage = "Base salary must be between 0 and 1,000,000")]
        [Display(Name = "Base Salary")]
        public decimal BaseSalary { get; set; }

        [Display(Name = "Bonus")]
        [Range(0, 1000000, ErrorMessage = "Bonus must be between 0 and 1,000,000")]
        public decimal Bonus { get; set; }

        [Display(Name = "Deductions")]
        [Range(0, 1000000, ErrorMessage = "Deductions must be between 0 and 1,000,000")]
        public decimal Deductions { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        public decimal NetSalary => BaseSalary + Bonus - Deductions;
    }
}