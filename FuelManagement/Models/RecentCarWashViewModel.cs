using System;

namespace FuelManagement.Models
{
    public class RecentCarWashViewModel
    {
        public int Id { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}