using System;
using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models
{
    public class FuelSaleEditViewModel : FuelSaleFormViewModel
    {
        [Required]
        public int Id { get; set; }

        // Only property that exists ONLY in Edit mode
        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}