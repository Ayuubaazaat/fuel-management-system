using FuelManagement.Data;
using FuelManagement.Models;
using FuelManagement.Models.Enums;
using FuelManagement.Models.ViewModels;
using FuelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuelManagement.Controllers
{
    [Authorize]
    public class RecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public RecordsController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // =============================================
        // INDEX
        // =============================================
        public async Task<IActionResult> Index(
            string? tab,
            string? moduleFilter,
            string? typeFilter,
            string? searchTerm,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            // Admin only
            if (!_authService.IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            const int pageSize = 20;
            tab ??= "activity";

            // ── ACTIVITY LOG ──────────────────────────────
            var activityQuery = _context.Notifications.AsQueryable();

            if (!string.IsNullOrEmpty(moduleFilter))
                activityQuery = activityQuery.Where(n => n.Module == moduleFilter);

            if (!string.IsNullOrEmpty(typeFilter) &&
                Enum.TryParse<NotificationType>(typeFilter, out var parsedType))
                activityQuery = activityQuery.Where(n => n.Type == parsedType);

            if (!string.IsNullOrEmpty(searchTerm))
                activityQuery = activityQuery.Where(n =>
                    n.Title.Contains(searchTerm) ||
                    n.Description.Contains(searchTerm) ||
                    n.Module.Contains(searchTerm));

            if (dateFrom.HasValue)
                activityQuery = activityQuery.Where(n => n.Timestamp >= dateFrom.Value);

            if (dateTo.HasValue)
                activityQuery = activityQuery.Where(n => n.Timestamp <= dateTo.Value.AddDays(1));

            var totalActivity = await activityQuery.CountAsync();
            var totalActivityPages = (int)Math.Ceiling(totalActivity / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalActivityPages > 0 ? totalActivityPages : 1));

            var activityLogs = await activityQuery
                .OrderByDescending(n => n.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ── LOGIN SESSIONS ────────────────────────────
            var loginQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                loginQuery = loginQuery.Where(u =>
                    u.FullName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm));

            if (dateFrom.HasValue)
                loginQuery = loginQuery.Where(u => u.LastLoginAt >= dateFrom.Value);

            if (dateTo.HasValue)
                loginQuery = loginQuery.Where(u => u.LastLoginAt <= dateTo.Value.AddDays(1));

            var loginSessions = await loginQuery
                .Where(u => u.LastLoginAt != null)
                .OrderByDescending(u => u.LastLoginAt)
                .Select(u => new LoginSessionViewModel
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    LastLoginAt = u.LastLoginAt,
                    LastLoginIp = u.LastLoginIp,
                    Device = u.LastLoginDevice,
                    LoginCount = u.LoginCount,
                    IsActive = u.IsActive,
                    AvatarUrl = u.ProfileImageUrl
                })
                .ToListAsync();

            // ── SUMMARY STATS ─────────────────────────────
            var today = DateTime.Today;

            ViewBag.Tab = tab;
            ViewBag.ModuleFilter = moduleFilter;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalActivityPages;
            ViewBag.TotalActivity = totalActivity;
            ViewBag.TotalLogins = loginSessions.Count;
            ViewBag.TodayActivity = await _context.Notifications
                                        .CountAsync(n => n.Timestamp.Date == today);
            ViewBag.TodayLogins = loginSessions
                                        .Count(l => l.LastLoginAt?.Date == today);

            ViewBag.Modules = await _context.Notifications
                .Select(n => n.Module)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            var viewModel = new RecordsViewModel
            {
                ActivityLogs = activityLogs,
                LoginSessions = loginSessions
            };

            return View(viewModel);
        }
    }
}