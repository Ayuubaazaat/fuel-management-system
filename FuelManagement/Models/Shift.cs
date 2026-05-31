using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace FuelManagement.Models
{
    public class Shift
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Shift Name")]
        public string ShiftName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Pump Number")]
        public string PumpNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Opening Meter")]
        public decimal OpeningMeter { get; set; }

        [Display(Name = "Closing Meter")]
        public decimal? ClosingMeter { get; set; }

        [Display(Name = "Cash Collected")]
        public decimal CashCollected { get; set; }

        public string Status { get; set; } = "Active";

        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Avatar stored path (saved in DB)
        public string? AvatarPath { get; set; }

        // File upload — NOT mapped to DB
        [NotMapped]
        public IFormFile? AvatarFile { get; set; }
    }
}