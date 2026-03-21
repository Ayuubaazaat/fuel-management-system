using System;
using System.Collections.Generic;
using System.Linq;

namespace FuelManagement.Models
{
    public class TripsViewModel
    {
        public TripFilterViewModel Filter { get; set; } = new();
        public List<TripListItemViewModel> TripItems { get; set; } = new();

        // Pagination properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        // Summary card data
        public int TotalTrips => TotalItems;
        public decimal TotalDistance { get; set; }
        public decimal TotalFuelUsed { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageEfficiency { get; set; }
        public decimal AverageDistance { get; set; }
        public decimal AverageCostPerTrip { get; set; }
        public int NewTripsThisMonth { get; set; }

        // Formatted values for display
        public string FormattedTotalDistance => $"{TotalDistance:N0} km";
        public string FormattedTotalFuelUsed => $"{TotalFuelUsed:N1} L";
        public string FormattedTotalCost => TotalCost.ToString("C");
        public string FormattedAverageEfficiency => $"{AverageEfficiency:F1} L/100km";
        public string FormattedAverageDistance => $"{AverageDistance:N0} km";
        public string FormattedAverageCostPerTrip => AverageCostPerTrip.ToString("C");

        // Pagination helpers
        public int StartIndex => (PageNumber - 1) * PageSize + 1;
        public int EndIndex => Math.Min(PageNumber * PageSize, TotalItems);
    }
}