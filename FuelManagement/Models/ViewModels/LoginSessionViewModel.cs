using System;

namespace FuelManagement.Models
{
    public class LoginSessionViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public string? Device { get; set; }
        public int LoginCount { get; set; }
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }

        public string Initials => FullName.Length >= 2
            ? FullName.Substring(0, 2).ToUpper()
            : FullName.ToUpper();

        public string TimeAgo
        {
            get
            {
                if (LastLoginAt == null) return "Never";
                var diff = DateTime.Now - LastLoginAt.Value;
                if (diff.TotalMinutes < 1) return "Just now";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
                if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
                return LastLoginAt.Value.ToString("MMM dd, yyyy");
            }
        }
    }
}