using FuelManagement.Models.Enums;
using FuelManagement.Models.ViewModels;
using FuelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FuelManagement.Controllers
{
    // DTO to receive JSON body with id
    public class NotificationIdRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(NotificationFilterViewModel filter)
        {
            filter.AvailableModules = new List<SelectListItem>
            {
                new() { Text = "All Modules",   Value = "All" },
                new() { Text = "Users",         Value = "Users" },
                new() { Text = "Fuel Clients",  Value = "Fuel Clients" },
                new() { Text = "Fuel Sales",    Value = "Fuel Sales" },
                new() { Text = "Inventory",     Value = "Inventory" },
                new() { Text = "Fleet Mgmt",    Value = "Fleet Mgmt" },
                new() { Text = "Trips",         Value = "Trips" },
                new() { Text = "Pumps",         Value = "Pumps" },
                new() { Text = "Compensation",  Value = "Compensation" },
                new() { Text = "Shifts", Value = "Shifts" },
                new() { Text = "Invoices",      Value = "Invoices" },
                new() { Text = "Receipts",      Value = "Receipts" },
            };

            filter.AvailableTypes = Enum.GetValues<NotificationType>()
                .Select(t => new SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                }).ToList();
            filter.AvailableTypes.Insert(0, new SelectListItem { Text = "All Types", Value = "" });

            filter.AvailableStatuses = new List<SelectListItem>
            {
                new() { Text = "All Status", Value = "" },
                new() { Text = "Unread",     Value = "0" },
                new() { Text = "Read",       Value = "1" }
            };

            filter.AvailableDateRanges = new List<SelectListItem>
            {
                new() { Text = "Last 24 Hours", Value = "24h" },
                new() { Text = "Last 7 Days",   Value = "7d" },
                new() { Text = "Last 30 Days",  Value = "30d" },
                new() { Text = "Last 90 Days",  Value = "90d" },
                new() { Text = "Custom Range",  Value = "custom" }
            };

            var notifications = await _notificationService.GetFilteredNotificationsAsync(filter);
            var totalCount = await _notificationService.GetTotalCountAsync(filter);
            var unreadCount = await _notificationService.GetUnreadCountAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.UnreadCount = unreadCount;
            ViewBag.CurrentPage = filter.Page;
            ViewBag.PageSize = filter.PageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            ViewBag.Filter = filter;

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            var notifications = await _notificationService.GetRecentNotificationsAsync(5);
            var unreadCount = await _notificationService.GetUnreadCountAsync();

            return Json(new
            {
                success = true,
                notifications = notifications.Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Description,
                    n.Module,
                    n.ModuleIcon,
                    Timestamp = n.Timestamp.ToString("MMM dd, h:mm tt"),
                    n.Type,
                    n.ActionUrl,
                    n.ActionText
                }),
                unreadCount
            });
        }

        // FIXED: accepts JSON body { "id": "..." } instead of query string
        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromBody] NotificationIdRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Id))
                return Json(new { success = false });

            var result = await _notificationService.MarkAsReadAsync(request.Id);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _notificationService.MarkAllAsReadAsync();
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> ClearHistory()
        {
            var result = await _notificationService.ClearHistoryAsync();
            return Json(new { success = result });
        }

        // FIXED: accepts JSON body { "id": "..." } instead of query string
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] NotificationIdRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Id))
                return Json(new { success = false });

            var result = await _notificationService.DeleteNotificationAsync(request.Id);
            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync();
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestNotification()
        {
            var modules = new[] {
                "Fuel Sales", "Inventory", "Pumps", "Invoices", "Compensation",
                "Fuel Clients", "Trips", "Users", "Fleet Mgmt", "Reports"
            };
            var titles = new[]
            {
                "New transaction recorded",
                "System alert",
                "Maintenance required",
                "Report generated",
                "Invoice updated",
                "Client registered",
                "Trip completed",
                "User activity"
            };

            var random = new Random();
            var module = modules[random.Next(modules.Length)];
            var title = titles[random.Next(titles.Length)];
            var types = new[] { NotificationType.Info, NotificationType.Success, NotificationType.Warning, NotificationType.Alert };
            var type = types[random.Next(types.Length)];

            var notification = await _notificationService.CreateNotificationAsync(
                title,
                $"This is a sample notification for {module} module.",
                module,
                type,
                $"/{module.Replace(" ", "")}",
                "View Details"
            );

            return Json(new { success = true, notification });
        }
    }
}