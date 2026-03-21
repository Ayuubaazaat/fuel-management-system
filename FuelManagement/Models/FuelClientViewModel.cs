using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class FuelClientViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Client ID")]
        public string? ClientId { get; set; } // Auto-generated, read-only

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Client type is required")]
        [Display(Name = "Client Type")]
        public string ClientType { get; set; } = "Individual";

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
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
    }
}