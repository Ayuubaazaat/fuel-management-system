using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class ReportsViewModel
    {
        public ReportFilterViewModel Filter { get; set; }
        public ReportSummaryViewModel Summary { get; set; }
        public ReportChartViewModel ChartData { get; set; }
        public List<ReportListItemViewModel> ReportItems { get; set; }
    }
}