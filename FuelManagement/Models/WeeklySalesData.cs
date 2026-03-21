using System;

namespace FuelManagement.Models
{
    public class WeeklySalesData
    {
        public string WeekLabel { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public string DisplayLabel { get; set; } = string.Empty;
        public decimal Sales { get; set; }
    }
}