using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class FuelClientFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? ClientType { get; set; }
        public string? Status { get; set; }
        public string? City { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // For dropdowns
        public List<string> AvailableClientTypes { get; set; } = new List<string>();
        public List<string> AvailableCities { get; set; } = new List<string>();
    }
}