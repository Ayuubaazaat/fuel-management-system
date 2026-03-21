using FuelManagement.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace FuelManagement.Models.ViewModels
{
    public class NotificationFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Module { get; set; }
        public NotificationType? Type { get; set; }
        public NotificationStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Timestamp";
        public bool SortDescending { get; set; } = true;

        public List<SelectListItem> AvailableModules { get; set; } = new();
        public List<SelectListItem> AvailableTypes { get; set; } = new();
        public List<SelectListItem> AvailableStatuses { get; set; } = new();
        public List<SelectListItem> AvailableDateRanges { get; set; } = new();
    }
}