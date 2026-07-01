using System;
using System.Collections.Generic;

namespace FuelManagement.Models.ViewModels
{
    public class RecordsViewModel
    {
        public List<Notification> ActivityLogs { get; set; } = new();
        public List<LoginSessionViewModel> LoginSessions { get; set; } = new();
    }
}