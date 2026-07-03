using System.ComponentModel.DataAnnotations;

namespace FuelManagement.Models.Enums
{
    public enum PaymentMethod
    {
        [Display(Name = "Evc +")]
        EvcPlus = 0,

        [Display(Name = "JEEB")]
        JEEB = 3,

        [Display(Name = "E-Dahab")]
        EDahab = 4
    }
}