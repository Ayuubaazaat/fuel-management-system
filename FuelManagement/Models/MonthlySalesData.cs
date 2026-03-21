using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelManagement.Models
{
    // Ensure there is only one definition of MonthlySalesData in this namespace.  
    public class MonthlySalesData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Sales { get; set; }
    }
}