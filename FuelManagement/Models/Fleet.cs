using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuelManagement.Models
{
    public class Fleet
    {
        [Key]
        [StringLength(20)]
        [Display(Name = "Vehicle ID")]
        public string VehicleId { get; set; }  // Primary key (like VH-001)

        [Required]
        [StringLength(100)]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Driver Name")]
        public string DriverName { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; }

        [Display(Name = "Total Trips")]
        public int TotalTrips { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Fuel Consumed (L)")]
        public decimal TotalFuelConsumed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fuel Efficiency (L/100km)")]
        public decimal FuelEfficiency { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Last Service Date")]
        public DateTime LastServiceDate { get; set; }

        [Display(Name = "Odometer (km)")]
        public int Odometer { get; set; }

        [Display(Name = "Next Service Due")]
        public DateTime NextServiceDue { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }  // Made nullable with ?

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }

        // Calculated field
        [NotMapped]
        public int ServiceDuePercentage
        {
            get
            {
                var daysUntilDue = (NextServiceDue - DateTime.UtcNow.Date).Days;
                if (daysUntilDue <= 0) return 100;
                if (daysUntilDue >= 30) return 0;
                return 100 - (int)((daysUntilDue / 30.0) * 100);
            }
        }
    }
}