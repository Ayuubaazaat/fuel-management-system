using System.Collections.Generic;

namespace FuelManagement.Models
{
    public class UserListViewModel
    {
        public IEnumerable<UserListItemViewModel> Users { get; set; } = new List<UserListItemViewModel>();
        public string SearchTerm { get; set; } = string.Empty;
        public string RoleFilter { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }

        // Pagination properties (if needed)
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }
}