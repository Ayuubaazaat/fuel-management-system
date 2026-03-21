using System;

namespace FuelManagement.Models
{
    public class FuelClientListItemViewModel
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string FullName => CompanyName ?? $"{FirstName} {LastName}";
        public string ClientType { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Status color for badge
        public string StatusColor => IsActive ? "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400" : "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-400";
    }
}