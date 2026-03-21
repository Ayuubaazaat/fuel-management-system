using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuelManagement.Models
{
    [Table("Invoices")]
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty; // e.g., INV-2024-001

        // Customer Information
        [Required]
        [StringLength(200)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; } = string.Empty; // Fleet, Corporate, Walk-in

        [StringLength(20)]
        [Display(Name = "Customer Phone")]
        public string? CustomerPhone { get; set; }

        [StringLength(100)]
        [Display(Name = "Customer Email")]
        public string? CustomerEmail { get; set; }

        [StringLength(200)]
        [Display(Name = "Customer Address")]
        public string? CustomerAddress { get; set; }

        // Fuel Details
        [Required]
        [StringLength(50)]
        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Liters")]
        public decimal TotalLiters { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "VAT")]
        public decimal VAT { get; set; } // 5% Somali VAT

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        // Payment Details
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Balance Due")]
        public decimal BalanceDue { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Unpaid"; // Paid, Unpaid, Partial, Overdue

        // Dates
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        // Metadata
        [StringLength(100)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Foreign Key to FuelSale (will connect later)
        public int? FuelSaleId { get; set; }

        // Navigation property for FuelSale (will connect later)
        [ForeignKey("FuelSaleId")]
        public virtual FuelSale? FuelSale { get; set; }

        // Navigation property for Receipts - COMMENTED OUT until Receipt model is created
        // public virtual ICollection<Receipt>? Receipts { get; set; }
    }
}