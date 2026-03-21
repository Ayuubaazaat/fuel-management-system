using FuelManagement.Models.Enums;
using System;

namespace FuelManagement.Models
{
    public class Notification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string ModuleIcon { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
        public bool IsGlobal { get; set; }

        // Reference IDs to link back to source data
        public string? ReferenceId { get; set; } // ID of the related record (FuelSaleId, InvoiceId, etc.)
        public string? ReferenceType { get; set; } // Type of reference (FuelSale, Invoice, etc.)
    }
}