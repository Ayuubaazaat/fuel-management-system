using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class CompensationFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }

        // Change these from read-only properties (using =>) to properties with setters
        public List<string> AvailableDepartments { get; set; } = new List<string>();
        public List<string> AvailableStatuses { get; set; } = new List<string>();
        public List<string> AvailableEmployees { get; set; } = new List<string>();

        // Optional: If you still want to provide default values, you can add a constructor
        public CompensationFilterViewModel()
        {
            // You can initialize with default values if needed
            // But these will be overwritten by the database values in the controller
        }
    }
}