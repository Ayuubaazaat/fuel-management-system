using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [Display(Name = "Bio")]
        public string? Bio { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public DateTime LastActive { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public bool PhoneVerified { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public int TotalLogins { get; set; }
        public string LastLoginIp { get; set; } = string.Empty;
        public string LoginDevice { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
    }

    public class SecurityViewModel
    {
        public bool TwoFactorEnabled { get; set; }
        public List<ActiveSession> ActiveSessions { get; set; } = new List<ActiveSession>();
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ActiveSession
    {
        public int Id { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastActive { get; set; }
        public bool IsCurrentSession { get; set; }
    }
}