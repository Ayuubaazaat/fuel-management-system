using System;
using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class FuelClientIndexViewModel
    {
        public FuelClientFilterViewModel Filter { get; set; } = new();
        public List<FuelClientListItemViewModel> Clients { get; set; } = new();

        // Summary stats
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int NewThisMonth { get; set; }
        public int FleetClients { get; set; }
        public int CorporateClients { get; set; }
        public int IndividualClients { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}