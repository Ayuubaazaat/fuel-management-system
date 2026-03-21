using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models.Enums
{
    public enum FuelSaleStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,

        [Display(Name = "Paid")]
        Paid = 1,

        [Display(Name = "Cancelled")]
        Cancelled = 2
    }
}