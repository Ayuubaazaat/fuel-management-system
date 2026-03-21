using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FuelManagement.Models.Enums;

namespace FuelManagement.Models
{
    public class FuelSale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Liters { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal PricePerLiter { get; set; }

        // Stored value (not computed column)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(20)]
        public string PumpNumber { get; set; } = string.Empty;

        // Stored as INT in database
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        // Stored as INT in database
        [Required]
        public FuelSaleStatus Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}