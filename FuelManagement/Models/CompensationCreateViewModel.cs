using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FuelManagement.Models
{
    public class CompensationCreateViewModel
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [RegularExpression(@"^EMP-\d{3}$",
            ErrorMessage = "Employee ID must be in format: EMP-001 (e.g., EMP-001, EMP-002)")]
        [Display(Name = "Employee ID")]
        [Remote(action: "CheckEmployeeId", controller: "Compensation", ErrorMessage = "Employee ID already exists")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        [Display(Name = "Employee Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Employee name must be between 2 and 100 characters")]
        public string EmployeeName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Please enter a valid phone number")]
        // Remote validation — calls CheckPhoneNumber on the server as user types
        [Remote(action: "CheckPhoneNumber", controller: "Compensation", ErrorMessage = "Phone number already exists")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [Display(Name = "Position")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Position must be between 2 and 100 characters")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Base salary is required")]
        [Range(0.01, 1000000, ErrorMessage = "Base salary must be between 0.01 and 1,000,000")]
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
        [FutureDate(ErrorMessage = "Payment date cannot be in the past")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }  // Make nullable with ?

        public decimal NetSalary => BaseSalary + Bonus - Deductions;
    }

    // Custom validation attribute for future date
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date >= DateTime.Today;
            }
            return false;
        }
    }
}