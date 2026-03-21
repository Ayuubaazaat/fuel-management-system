using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // ===== NEW PROPERTIES FOR PROFILE =====
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public int LoginCount { get; set; }

        [MaxLength(50)]
        public string? LastLoginIp { get; set; }

        [MaxLength(100)]
        public string? LastLoginDevice { get; set; }

        public DateTime? LastLoginAt { get; set; }
    }
}