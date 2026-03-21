using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuelManagement.Models
{
    public class FuelClient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; } = string.Empty; // e.g., CLI-001

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Client Type")]
        public string ClientType { get; set; } = "Individual"; // Individual, Fleet, Corporate

        [Required]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(50)]
        [Display(Name = "State")]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Country")]
        public string? Country { get; set; } = "Somalia";

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.MultilineText)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Audit fields
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }
    }
}