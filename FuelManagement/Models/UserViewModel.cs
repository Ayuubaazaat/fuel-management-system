using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Join Date")]
        public DateTime JoinDate { get; set; }

        [Display(Name = "Last Login")]
        public DateTime? LastLogin { get; set; }

        [Display(Name = "Last Login IP")]
        public string? LastLoginIp { get; set; }

        [Display(Name = "Login Device")]
        public string? LoginDevice { get; set; }

        [Display(Name = "Total Logins")]
        public int TotalLogins { get; set; }

        [Display(Name = "Email Verified")]
        public bool IsEmailVerified { get; set; }

        [Display(Name = "Profile Image")]
        public string? ProfileImageUrl { get; set; }
    }
}