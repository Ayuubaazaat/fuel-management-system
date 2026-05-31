using FuelManagement.Data;
using FuelManagement.Models;
using FuelManagement.Models.Enums;
using FuelManagement.Models.ViewModels;
using FuelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FuelManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly Dictionary<string, string> ModuleIcons = new()
        {
            { "Dashboard",    "fa-chart-pie" },
            { "Users",        "fa-users" },
            { "Fuel Clients", "fa-user-tie" },
            { "Fuel Sales",   "fa-gas-pump" },
            { "Inventory",    "fa-warehouse" },
            { "Fleet Mgmt",   "fa-truck" },
            { "Trips",        "fa-route" },
            { "Pumps",        "fa-oil-can" },
            { "Compensation", "fa-hand-holding-usd" },
            { "Invoices",     "fa-file-invoice-dollar" },
            { "Receipts",     "fa-receipt" },
            { "Reports",      "fa-chart-line" },
            { "Settings",     "fa-cog" },
            { "Shifts",       "fa-clock" } // [ADDED] Icon for the Shifts module
        };

        public NotificationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "anonymous";
        }

        private static NotificationViewModel MapToViewModel(Notification n) => new()
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Description = n.Description,
            Module = n.Module,
            ModuleIcon = n.ModuleIcon,
            Timestamp = n.Timestamp,
            Status = n.Status,
            Type = n.Type,
            ActionUrl = n.ActionUrl,
            ActionText = n.ActionText,
            IsGlobal = n.IsGlobal
        };

        public async Task<List<NotificationViewModel>> GetRecentNotificationsAsync(int count = 5)
        {
            var userId = GetCurrentUserId();

            var notifications = await _context.Notifications
                .Where(n => (n.UserId == userId || n.IsGlobal)
                         && n.Status == NotificationStatus.Unread)
                .OrderByDescending(n => n.Timestamp)
                .Take(count)
                .ToListAsync();

            return notifications.Select(MapToViewModel).ToList();
        }

        public async Task<NotificationViewModel?> GetNotificationByIdAsync(string id)
        {
            var n = await _context.Notifications.FindAsync(id);
            return n == null ? null : MapToViewModel(n);
        }

        public async Task<List<NotificationViewModel>> GetFilteredNotificationsAsync(NotificationFilterViewModel filter)
        {
            var userId = GetCurrentUserId();

            var query = _context.Notifications
                .Where(n => n.UserId == userId || n.IsGlobal)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(n =>
                    n.Title.Contains(filter.SearchTerm) ||
                    n.Description.Contains(filter.SearchTerm));

            if (!string.IsNullOrWhiteSpace(filter.Module) && filter.Module != "All")
                query = query.Where(n => n.Module == filter.Module);

            if (filter.Type.HasValue)
                query = query.Where(n => n.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                query = query.Where(n => n.Status == filter.Status.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.Timestamp >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.Timestamp <= filter.DateTo.Value);

            query = filter.SortDescending
                ? query.OrderByDescending(n => n.Timestamp)
                : query.OrderBy(n => n.Timestamp);

            var result = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return result.Select(MapToViewModel).ToList();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var userId = GetCurrentUserId();
            return await _context.Notifications
                .CountAsync(n => (n.UserId == userId || n.IsGlobal)
                              && n.Status == NotificationStatus.Unread);
        }

        public async Task<NotificationViewModel> CreateNotificationAsync(
            string title, string description, string module,
            NotificationType type,
            string? actionUrl = null,
            string? actionText = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = GetCurrentUserId(),
                Title = title,
                Description = description,
                Module = module,
                ModuleIcon = ModuleIcons.ContainsKey(module) ? ModuleIcons[module] : "fa-bell",
                Timestamp = DateTime.Now,
                Status = NotificationStatus.Unread,
                Type = type,
                ActionUrl = actionUrl,
                ActionText = actionText,
                IsGlobal = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return MapToViewModel(notification);
        }

        public async Task<bool> MarkAsReadAsync(string id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            notification.Status = NotificationStatus.Read;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            var userId = GetCurrentUserId();
            var notifications = await _context.Notifications
                .Where(n => (n.UserId == userId || n.IsGlobal)
                         && n.Status == NotificationStatus.Unread)
                .ToListAsync();

            notifications.ForEach(n => n.Status = NotificationStatus.Read);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearHistoryAsync()
        {
            var userId = GetCurrentUserId();
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(string id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalCountAsync(NotificationFilterViewModel filter)
        {
            var userId = GetCurrentUserId();
            var query = _context.Notifications
                .Where(n => n.UserId == userId || n.IsGlobal)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(n =>
                    n.Title.Contains(filter.SearchTerm) ||
                    n.Description.Contains(filter.SearchTerm));

            if (!string.IsNullOrWhiteSpace(filter.Module) && filter.Module != "All")
                query = query.Where(n => n.Module == filter.Module);

            if (filter.Type.HasValue)
                query = query.Where(n => n.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                query = query.Where(n => n.Status == filter.Status.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.Timestamp >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.Timestamp <= filter.DateTo.Value);

            return await query.CountAsync();
        }

        // ── Module-specific helpers ──────────────────────────────────────

        public async Task<NotificationViewModel> CreateFuelSaleNotification(
            string invoiceNumber, string customerName, decimal liters, decimal amount, string actionUrl)
            => await CreateNotificationAsync(
                $"Fuel Sale Completed: {invoiceNumber}",
                $"Sale of {liters:N0}L to {customerName} for {amount:C}",
                "Fuel Sales", NotificationType.Success, actionUrl, "View Sale");

        public async Task<NotificationViewModel> CreateLowInventoryAlert(
            string tankId, string fuelType, decimal currentStock, decimal capacity, decimal percentage, string actionUrl)
            => await CreateNotificationAsync(
                $"Low Inventory Alert: {tankId}",
                $"{fuelType} tank is at {percentage:F1}% ({currentStock:N0}L / {capacity:N0}L). Status: {(percentage <= 10 ? "Critical" : "Low")}",
                "Inventory",
                percentage <= 10 ? NotificationType.Alert : NotificationType.Warning,
                actionUrl,
                percentage <= 10 ? "Order Now" : "View Tank");

        public async Task<NotificationViewModel> CreateInventoryUpdateNotification(
            string tankId, string fuelType, decimal newStock, string actionUrl)
            => await CreateNotificationAsync(
                $"Inventory Updated: {tankId}",
                $"{fuelType} stock updated to {newStock:N0}L",
                "Inventory", NotificationType.Info, actionUrl, "View Inventory");

        public async Task<NotificationViewModel> CreateInvoiceCreatedNotification(
            string invoiceNumber, string customerName, decimal amount, string actionUrl)
            => await CreateNotificationAsync(
                $"New Invoice: {invoiceNumber}",
                $"Invoice for {customerName} created for {amount:C}",
                "Invoices", NotificationType.Info, actionUrl, "View Invoice");

        public async Task<NotificationViewModel> CreateInvoicePaidNotification(
            string invoiceNumber, string customerName, decimal amount, string actionUrl)
            => await CreateNotificationAsync(
                $"Invoice Paid: {invoiceNumber}",
                $"Payment of {amount:C} received from {customerName}",
                "Invoices", NotificationType.Success, actionUrl, "View Invoice");

        public async Task<NotificationViewModel> CreateOverdueInvoiceNotification(
            string invoiceNumber, string customerName, decimal amount, int daysOverdue, string actionUrl)
            => await CreateNotificationAsync(
                $"Overdue Invoice: {invoiceNumber}",
                $"Invoice for {customerName} is {daysOverdue} days overdue. Amount: {amount:C}",
                "Invoices", NotificationType.Alert, actionUrl, "Send Reminder");

        public async Task<NotificationViewModel> CreatePumpStatusNotification(
            string pumpId, string pumpName, string oldStatus, string newStatus, string actionUrl)
            => await CreateNotificationAsync(
                $"Pump Status Changed: {pumpId}",
                $"{pumpName} status changed from {oldStatus} to {newStatus}",
                "Pumps",
                newStatus == "Maintenance" ? NotificationType.Warning :
                newStatus == "Inactive" ? NotificationType.Alert : NotificationType.Info,
                actionUrl, "View Pump");

        public async Task<NotificationViewModel> CreatePumpMaintenanceDueNotification(
            string pumpId, string pumpName, DateTime dueDate, string actionUrl)
        {
            var days = (dueDate - DateTime.Today).Days;
            return await CreateNotificationAsync(
                $"Maintenance Due: {pumpId}",
                $"{pumpName} requires maintenance in {days} days (Due: {dueDate:MMM dd, yyyy})",
                "Pumps",
                days <= 3 ? NotificationType.Alert : NotificationType.Warning,
                actionUrl, "Schedule Now");
        }

        public async Task<NotificationViewModel> CreateTripCompletedNotification(
            string tripId, string vehicleName, string driverName, decimal distance, decimal fuelUsed, string actionUrl)
            => await CreateNotificationAsync(
                $"Trip Completed: {tripId}",
                $"{vehicleName} completed {distance:N0}km trip. Fuel used: {fuelUsed:N1}L",
                "Trips", NotificationType.Success, actionUrl, "View Trip");

        public async Task<NotificationViewModel> CreateTripAlertNotification(
            string tripId, string vehicleName, string alertType, string description, string actionUrl)
            => await CreateNotificationAsync(
                $"Trip Alert: {tripId}",
                $"{vehicleName} - {description}",
                "Trips", NotificationType.Warning, actionUrl, "View Details");

        public async Task<NotificationViewModel> CreateCompensationCreatedNotification(
            string employeeName, decimal amount, string period, string actionUrl)
            => await CreateNotificationAsync(
                "Compensation Record Created",
                $"Compensation for {employeeName}: {amount:C} for {period}",
                "Compensation", NotificationType.Info, actionUrl, "View Record");

        public async Task<NotificationViewModel> CreateCompensationPaidNotification(
            string employeeName, decimal amount, string actionUrl)
            => await CreateNotificationAsync(
                "Compensation Paid",
                $"Payment of {amount:C} processed for {employeeName}",
                "Compensation", NotificationType.Success, actionUrl, "View Record");

        public async Task<NotificationViewModel> CreateClientCreatedNotification(
            string clientName, string clientId, string clientType, string actionUrl)
            => await CreateNotificationAsync(
                $"New Client Registered: {clientId}",
                $"{clientType} client '{clientName}' has been registered",
                "Fuel Clients", NotificationType.Success, actionUrl, "View Client");

        public async Task<NotificationViewModel> CreateClientUpdatedNotification(
            string clientName, string clientId, string actionUrl)
            => await CreateNotificationAsync(
                $"Client Updated: {clientId}",
                $"Client '{clientName}' information has been updated",
                "Fuel Clients", NotificationType.Info, actionUrl, "View Client");

        public async Task<NotificationViewModel> CreateFleetMaintenanceDueNotification(
            string vehicleId, string vehicleName, DateTime dueDate, string actionUrl)
        {
            var days = (dueDate - DateTime.Today).Days;
            return await CreateNotificationAsync(
                $"Service Due: {vehicleId}",
                $"{vehicleName} requires service in {days} days (Due: {dueDate:MMM dd, yyyy})",
                "Fleet Mgmt",
                days <= 3 ? NotificationType.Alert : NotificationType.Warning,
                actionUrl, "Schedule Now");
        }

        public async Task<NotificationViewModel> CreateFleetStatsUpdatedNotification(
            string vehicleId, string vehicleName, int totalTrips, decimal totalFuel, string actionUrl)
            => await CreateNotificationAsync(
                $"Fleet Stats Updated: {vehicleId}",
                $"{vehicleName} now has {totalTrips} trips, {totalFuel:N1}L total fuel consumed",
                "Fleet Mgmt", NotificationType.Info, actionUrl, "View Vehicle");

        public async Task<NotificationViewModel> CreateUserCreatedNotification(
            string userName, string userRole, string actionUrl)
            => await CreateNotificationAsync(
                "New User Created",
                $"User '{userName}' has been added as {userRole}",
                "Users", NotificationType.Success, actionUrl, "View User");

        public async Task<NotificationViewModel> CreateUserRoleUpdatedNotification(
            string userName, string oldRole, string newRole, string actionUrl)
            => await CreateNotificationAsync(
                "User Role Updated",
                $"User '{userName}' promoted from {oldRole} to {newRole}",
                "Users", NotificationType.Info, actionUrl, "View Profile");

        public async Task<NotificationViewModel> CreateReceiptGeneratedNotification(
            string receiptNo, string customerName, decimal amount, string actionUrl)
            => await CreateNotificationAsync(
                $"New Receipt: {receiptNo}",
                $"Receipt for {customerName} generated for {amount:C}",
                "Receipts", NotificationType.Info, actionUrl, "View Receipt");

        public async Task<NotificationViewModel> CreateSystemNotification(
            string title, string description, NotificationType type,
            string? actionUrl = null,
            string? actionText = null,
            bool isGlobal = true)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = GetCurrentUserId(),
                Title = title,
                Description = description,
                Module = "Settings",
                ModuleIcon = "fa-cog",
                Timestamp = DateTime.Now,
                Status = NotificationStatus.Unread,
                Type = type,
                ActionUrl = actionUrl,
                ActionText = actionText,
                IsGlobal = isGlobal
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return MapToViewModel(notification);
        }

        // ── SHIFTS notifications (newly added) ───────────────────────────

        // [ADDED] Fired when a new shift is created/started by a user
        public async Task<NotificationViewModel> CreateShiftStartedNotification(
            int shiftId, string shiftName, string assignedTo, string pumpNumber, string actionUrl)
            => await CreateNotificationAsync(
                // [ADDED] Title includes shift name to make the notification identifiable at a glance
                $"Shift Started: {shiftName}",
                // [ADDED] Description shows who is assigned and which pump, matching fields from Shift.cs
                $"Shift '{shiftName}' assigned to {assignedTo} on pump {pumpNumber} has started",
                // [ADDED] Module name must match exactly the sidebar entry we will add in Index.cshtml
                "Shifts",
                // [ADDED] Success type because starting a shift is a positive/normal action
                NotificationType.Success,
                // [ADDED] Action URL links to the Details page of this specific shift
                actionUrl,
                // [ADDED] Button label shown on the notification card
                "View Shift");

        // [ADDED] Fired when an existing shift is edited/updated
        public async Task<NotificationViewModel> CreateShiftUpdatedNotification(
            int shiftId, string shiftName, string assignedTo, string actionUrl)
            => await CreateNotificationAsync(
                // [ADDED] Title identifies which shift was updated
                $"Shift Updated: {shiftName}",
                // [ADDED] Description shows who the shift belongs to so admins know at a glance
                $"Shift '{shiftName}' assigned to {assignedTo} has been updated",
                // [ADDED] Same module string used throughout — must be consistent
                "Shifts",
                // [ADDED] Info type because an update is neutral, not critical
                NotificationType.Info,
                actionUrl,
                // [ADDED] Button label for the notification action link
                "View Shift");

        // [ADDED] Fired when a shift is closed with final meter reading and cash collected
        public async Task<NotificationViewModel> CreateShiftClosedNotification(
            int shiftId, string shiftName, string assignedTo, decimal closingMeter, decimal cashCollected, string actionUrl)
            => await CreateNotificationAsync(
                // [ADDED] Title shows the shift name so it's easy to identify in the list
                $"Shift Closed: {shiftName}",
                // [ADDED] Description includes closing meter and cash — key financial data from CloseShift action
                $"Shift '{shiftName}' by {assignedTo} closed. Meter: {closingMeter:N1}, Cash: {cashCollected:C}",
                "Shifts",
                // [ADDED] Warning type to draw attention — closing a shift is an important financial event
                NotificationType.Warning,
                actionUrl,
                // [ADDED] Button label for the notification action link
                "View Details");

        // [ADDED] Fired when a shift record is permanently deleted
        public async Task<NotificationViewModel> CreateShiftDeletedNotification(
            int shiftId, string shiftName, string assignedTo, string actionUrl)
            => await CreateNotificationAsync(
                // [ADDED] Title clearly states deletion happened
                $"Shift Deleted: {shiftName}",
                // [ADDED] Description preserves context of who the shift belonged to before deletion
                $"Shift '{shiftName}' assigned to {assignedTo} has been deleted",
                "Shifts",
                // [ADDED] Alert type because deletion is destructive and irreversible
                NotificationType.Alert,
                // [ADDED] Action URL points to Shifts index since the record no longer exists
                actionUrl,
                // [ADDED] Button label for the notification action link
                "View Shifts");
    }
}