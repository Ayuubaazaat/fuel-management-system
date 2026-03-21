using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        // Populated after email lookup
        public string FoundUserFullName { get; set; }
        public string FoundUserEmail { get; set; }
        public string FoundUserProfileImage { get; set; }
        public bool UserFound { get; set; }
        public bool NotFound { get; set; }
        public bool PasswordReset { get; set; }
    }
}