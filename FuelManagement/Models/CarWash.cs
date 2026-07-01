using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuelManagement.Models
{
    public class CarWash
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle plate is required")]
        [StringLength(20, ErrorMessage = "Plate cannot exceed 20 characters")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Plate must be uppercase letters, numbers or dashes only")]
        [Display(Name = "Vehicle Plate")]
        public string VehiclePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type is required")]
        [StringLength(50)]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service type is required")]
        [StringLength(50)]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 99999, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(20)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "EVC+";

        [Required]
        [StringLength(30)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Unpaid";

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Completed At")]
        public DateTime? CompletedAt { get; set; }

        [StringLength(100)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }
    }
}