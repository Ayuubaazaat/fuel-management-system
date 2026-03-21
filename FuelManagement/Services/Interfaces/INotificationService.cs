using FuelManagement.Models.Enums;
using FuelManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuelManagement.Services.Interfaces
{
    public interface INotificationService
    {
        // Basic CRUD methods
        Task<List<NotificationViewModel>> GetRecentNotificationsAsync(int count = 5);
        Task<NotificationViewModel?> GetNotificationByIdAsync(string id);
        Task<List<NotificationViewModel>> GetFilteredNotificationsAsync(NotificationFilterViewModel filter);
        Task<int> GetUnreadCountAsync();
        Task<NotificationViewModel> CreateNotificationAsync(string title, string description, string module, NotificationType type, string? actionUrl = null, string? actionText = null);
        Task<bool> MarkAsReadAsync(string id);
        Task<bool> MarkAllAsReadAsync();
        Task<bool> ClearHistoryAsync();
        Task<bool> DeleteNotificationAsync(string id);
        Task<int> GetTotalCountAsync(NotificationFilterViewModel filter);

        // ==================== MODULE-SPECIFIC METHODS ====================

        // FUEL SALES
        Task<NotificationViewModel> CreateFuelSaleNotification(string invoiceNumber, string customerName, decimal liters, decimal amount, string actionUrl);

        // INVENTORY
        Task<NotificationViewModel> CreateLowInventoryAlert(string tankId, string fuelType, decimal currentStock, decimal capacity, decimal percentage, string actionUrl);
        Task<NotificationViewModel> CreateInventoryUpdateNotification(string tankId, string fuelType, decimal newStock, string actionUrl);

        // INVOICES
        Task<NotificationViewModel> CreateInvoiceCreatedNotification(string invoiceNumber, string customerName, decimal amount, string actionUrl);
        Task<NotificationViewModel> CreateInvoicePaidNotification(string invoiceNumber, string customerName, decimal amount, string actionUrl);
        Task<NotificationViewModel> CreateOverdueInvoiceNotification(string invoiceNumber, string customerName, decimal amount, int daysOverdue, string actionUrl);

        // PUMPS
        Task<NotificationViewModel> CreatePumpStatusNotification(string pumpId, string pumpName, string oldStatus, string newStatus, string actionUrl);
        Task<NotificationViewModel> CreatePumpMaintenanceDueNotification(string pumpId, string pumpName, DateTime dueDate, string actionUrl);

        // TRIPS
        Task<NotificationViewModel> CreateTripCompletedNotification(string tripId, string vehicleName, string driverName, decimal distance, decimal fuelUsed, string actionUrl);
        Task<NotificationViewModel> CreateTripAlertNotification(string tripId, string vehicleName, string alertType, string description, string actionUrl);

        // COMPENSATION
        Task<NotificationViewModel> CreateCompensationCreatedNotification(string employeeName, decimal amount, string period, string actionUrl);
        Task<NotificationViewModel> CreateCompensationPaidNotification(string employeeName, decimal amount, string actionUrl);

        // FUEL CLIENTS
        Task<NotificationViewModel> CreateClientCreatedNotification(string clientName, string clientId, string clientType, string actionUrl);
        Task<NotificationViewModel> CreateClientUpdatedNotification(string clientName, string clientId, string actionUrl);

        // FLEET MANAGEMENT
        Task<NotificationViewModel> CreateFleetMaintenanceDueNotification(string vehicleId, string vehicleName, DateTime dueDate, string actionUrl);
        Task<NotificationViewModel> CreateFleetStatsUpdatedNotification(string vehicleId, string vehicleName, int totalTrips, decimal totalFuel, string actionUrl);

        // USERS
        Task<NotificationViewModel> CreateUserCreatedNotification(string userName, string userRole, string actionUrl);
        Task<NotificationViewModel> CreateUserRoleUpdatedNotification(string userName, string oldRole, string newRole, string actionUrl);

        // RECEIPTS
        Task<NotificationViewModel> CreateReceiptGeneratedNotification(string receiptNo, string customerName, decimal amount, string actionUrl);

        // SETTINGS
        Task<NotificationViewModel> CreateSystemNotification(string title, string description, NotificationType type, string? actionUrl = null, string? actionText = null, bool isGlobal = true);
    }
}