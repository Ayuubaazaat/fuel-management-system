using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuelManagement.Models
{
    [Table("Trips")]
    public class Trip
    {
        [Key]
        [StringLength(20)]
        [Display(Name = "Trip ID")]
        public string TripId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Vehicle ID")]
        public string VehicleId { get; set; } = string.Empty;  // Foreign key to Fleet

        [Required]
        [StringLength(100)]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Trip Date")]
        public DateTime TripDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Distance (km)")]
        public decimal Distance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fuel Used (L)")]
        public decimal FuelUsed { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fuel Efficiency (L/100km)")]
        public decimal FuelEfficiency { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost ($)")]
        public decimal Cost { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("VehicleId")]
        public virtual Fleet? Vehicle { get; set; }
    }
}