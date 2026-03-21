using System;

namespace FuelManagement.Models
{
    public class UserListItemViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinDate { get; set; }

        // Computed property for display
        public string FullName => $"{FirstName} {LastName}";
        public string Initials => $"{FirstName?[0]}{LastName?[0]}".ToUpper();
        public string JoinDateFormatted => JoinDate.ToString("MMM yyyy");
    }
}