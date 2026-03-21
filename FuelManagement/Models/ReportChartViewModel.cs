using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class ReportChartViewModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> RevenueData { get; set; }
        public List<decimal> VATData { get; set; }
        public List<decimal> OutstandingData { get; set; }

        public decimal MaxRevenue => RevenueData?.Count > 0 ? GetMax(RevenueData) : 0;
        public decimal MaxVAT => VATData?.Count > 0 ? GetMax(VATData) : 0;
        public decimal MaxOutstanding => OutstandingData?.Count > 0 ? GetMax(OutstandingData) : 0;

        private decimal GetMax(List<decimal> data)
        {
            if (data == null || data.Count == 0) return 0;
            var max = data.Max();
            // Round up to nearest 50000 for chart scaling using decimal
            return decimal.Ceiling(max / 50000m) * 50000m;
        }
    }
}