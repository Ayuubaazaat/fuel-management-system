using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "Evc +")]
        EvcPlus,

        Waafi,
        Somtel,
        Somnet,

        [Display(Name = "E-Dahab")]
        EDahab
    }
}