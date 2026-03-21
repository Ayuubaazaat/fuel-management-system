using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models.ViewModels
{
    public class SwitchAccountViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Profile Image")]
        public string? ProfileImageUrl { get; set; }

        public string Role { get; set; } = string.Empty;

        // Optional helper property (very useful for UI)
        public string DisplayImage =>
            string.IsNullOrEmpty(ProfileImageUrl)
                ? "/images/avatars/default.jpg"
                : ProfileImageUrl;
    }
}