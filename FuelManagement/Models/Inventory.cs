using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuelManagement.Models
{
    public class Inventory
    {
        [Key]
        [StringLength(20)]
        public string TankId { get; set; }  // This is now the primary key

        [Required]
        [StringLength(50)]
        public string FuelType { get; set; }

        [Required]
        public decimal Capacity { get; set; }

        [Required]
        public decimal CurrentStock { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal PricePerLiter { get; set; } = 3.50m;

        public DateTime LastUpdated { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        public DateTime? LastRefillDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }  // Made nullable with ?

        [NotMapped]
        public decimal FillLevelPercentage =>
            Capacity > 0 ? (CurrentStock / Capacity) * 100 : 0;

        [NotMapped]
        public decimal StockValue => CurrentStock * PricePerLiter;
    }
}