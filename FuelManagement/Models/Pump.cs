using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuelManagement.Models
{
    [Table("Pumps")]
    public class Pump
    {
        [Key]
        [StringLength(10)]
        [Display(Name = "Pump ID")]
        [RegularExpression(@"^PMP-\d{3}$", ErrorMessage = "Pump ID must be in format PMP-001")]
        public string PumpId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Pump Name/Description")]
        public string PumpName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tank")]
        public string TankId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Fuel Dispensed (L)")]
        public decimal TotalFuelDispensed { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Flow Rate (L/min)")]
        public decimal FlowRate { get; set; } = 0;

        [Required]
        [Display(Name = "Transaction Count")]
        public int TransactionCount { get; set; } = 0;

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Last Maintenance Date")]
        [DataType(DataType.Date)]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "Next Maintenance Due")]
        [DataType(DataType.Date)]
        public DateTime? NextMaintenanceDue { get; set; }

        [Required]
        [Display(Name = "Installation Date")]
        [DataType(DataType.Date)]
        public DateTime InstallationDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
    }
}