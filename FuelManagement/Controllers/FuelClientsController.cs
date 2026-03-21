using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FuelManagement.Data;
using FuelManagement.Models;
using FuelManagement.Services.Interfaces;
using FuelManagement.Models.Enums;

namespace FuelManagement.Controllers
{
    public class FuelClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public FuelClientsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: FuelClients
        public async Task<IActionResult> Index(FuelClientFilterViewModel filter, int page = 1)
        {
            int pageSize = 10;

            // Base query
            var query = _context.FuelClients.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(filter.SearchTerm) ||
                    c.LastName.Contains(filter.SearchTerm) ||
                    c.CompanyName!.Contains(filter.SearchTerm) ||
                    c.Phone.Contains(filter.SearchTerm) ||
                    c.Email!.Contains(filter.SearchTerm) ||
                    c.ClientId.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filter.ClientType))
            {
                query = query.Where(c => c.ClientType == filter.ClientType);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                bool isActive = filter.Status == "Active";
                query = query.Where(c => c.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(c => c.City == filter.City);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt.Date >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt.Date <= filter.ToDate.Value.Date);
            }

            // Get filter options for dropdowns
            filter.AvailableClientTypes = await _context.FuelClients
                .Select(c => c.ClientType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            filter.AvailableCities = await _context.FuelClients
                .Where(c => c.City != null)
                .Select(c => c.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Get total count for pagination
            int totalCount = await query.CountAsync();

            // FIXED: Order by ClientId ascending (CLI-001, CLI-002, CLI-003...)
            var clients = await query
                .OrderBy(c => c.ClientId)  // Changed from OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new FuelClientListItemViewModel
                {
                    Id = c.Id,
                    ClientId = c.ClientId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    CompanyName = c.CompanyName,
                    ClientType = c.ClientType,
                    Phone = c.Phone,
                    Email = c.Email,
                    City = c.City,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            // Calculate summary stats
            var stats = await _context.FuelClients
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(c => c.IsActive),
                    NewThisMonth = g.Count(c => c.CreatedAt.Month == DateTime.Now.Month && c.CreatedAt.Year == DateTime.Now.Year),
                    Fleet = g.Count(c => c.ClientType == "Fleet"),
                    Corporate = g.Count(c => c.ClientType == "Corporate"),
                    Individual = g.Count(c => c.ClientType == "Individual")
                })
                .FirstOrDefaultAsync();

            var viewModel = new FuelClientIndexViewModel
            {
                Filter = filter,
                Clients = clients,
                TotalClients = stats?.Total ?? 0,
                ActiveClients = stats?.Active ?? 0,
                NewThisMonth = stats?.NewThisMonth ?? 0,
                FleetClients = stats?.Fleet ?? 0,
                CorporateClients = stats?.Corporate ?? 0,
                IndividualClients = stats?.Individual ?? 0,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: FuelClients/Create
        public IActionResult Create()
        {
            return View(new FuelClientViewModel());
        }

        // POST: FuelClients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FuelClientViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if phone is unique
                if (await _context.FuelClients.AnyAsync(c => c.Phone == viewModel.Phone))
                {
                    ModelState.AddModelError("Phone", "This phone number is already registered.");
                    return View(viewModel);
                }

                // Generate Client ID
                string clientId = await GenerateClientId();

                var client = new FuelClient
                {
                    ClientId = clientId,
                    FirstName = viewModel.FirstName.Trim(),
                    LastName = viewModel.LastName.Trim(),
                    CompanyName = viewModel.CompanyName?.Trim(),
                    ClientType = viewModel.ClientType,
                    Phone = viewModel.Phone.Trim(),
                    Email = viewModel.Email?.Trim().ToLower(),
                    Address = viewModel.Address?.Trim(),
                    City = viewModel.City?.Trim(),
                    State = viewModel.State?.Trim(),
                    PostalCode = viewModel.PostalCode?.Trim(),
                    Country = viewModel.Country,
                    IsActive = viewModel.IsActive,
                    Notes = viewModel.Notes?.Trim(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.FuelClients.Add(client);
                await _context.SaveChangesAsync();

                // ===== CREATE NOTIFICATION FOR NEW CLIENT REGISTERED =====
                await _notificationService.CreateClientCreatedNotification(
                    clientName: client.FullName,
                    clientId: client.ClientId,
                    clientType: client.ClientType,
                    actionUrl: $"/FuelClients/Details/{client.Id}"
                );
                // =========================================================

                TempData["Success"] = $"Client {client.FullName} created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: FuelClients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.FuelClients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new FuelClientViewModel
            {
                Id = client.Id,
                ClientId = client.ClientId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                CompanyName = client.CompanyName,
                ClientType = client.ClientType,
                Phone = client.Phone,
                Email = client.Email,
                Address = client.Address,
                City = client.City,
                State = client.State,
                PostalCode = client.PostalCode,
                Country = client.Country,
                IsActive = client.IsActive,
                Notes = client.Notes
            };

            return View(viewModel);
        }

        // POST: FuelClients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelClientViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var client = await _context.FuelClients.FindAsync(id);
                if (client == null)
                {
                    return NotFound();
                }

                // Check if phone is unique (excluding current client)
                if (await _context.FuelClients.AnyAsync(c => c.Phone == viewModel.Phone && c.Id != id))
                {
                    ModelState.AddModelError("Phone", "This phone number is already registered.");
                    return View(viewModel);
                }

                // Store old values for notification
                var oldFullName = client.FullName;
                var oldIsActive = client.IsActive;

                // Update fields
                client.FirstName = viewModel.FirstName.Trim();
                client.LastName = viewModel.LastName.Trim();
                client.CompanyName = viewModel.CompanyName?.Trim();
                client.ClientType = viewModel.ClientType;
                client.Phone = viewModel.Phone.Trim();
                client.Email = viewModel.Email?.Trim().ToLower();
                client.Address = viewModel.Address?.Trim();
                client.City = viewModel.City?.Trim();
                client.State = viewModel.State?.Trim();
                client.PostalCode = viewModel.PostalCode?.Trim();
                client.Country = viewModel.Country;
                client.IsActive = viewModel.IsActive;
                client.Notes = viewModel.Notes?.Trim();
                client.UpdatedAt = DateTime.Now;
                client.UpdatedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                // ===== CREATE NOTIFICATION FOR CLIENT UPDATED =====
                string updateDescription = oldFullName != client.FullName
                    ? $"Client name changed from {oldFullName} to {client.FullName}"
                    : $"Client {client.FullName} information has been updated";

                if (oldIsActive != viewModel.IsActive)
                {
                    updateDescription = $"Client {client.FullName} is now {(viewModel.IsActive ? "Active" : "Inactive")}";
                }

                await _notificationService.CreateClientUpdatedNotification(
                    clientName: client.FullName,
                    clientId: client.ClientId,
                    actionUrl: $"/FuelClients/Details/{client.Id}"
                );

                // Additional notification for status change if needed
                if (oldIsActive != viewModel.IsActive)
                {
                    await _notificationService.CreateNotificationAsync(
                        title: $"Client Status Changed: {client.ClientId}",
                        description: $"Client {client.FullName} is now {(viewModel.IsActive ? "Active" : "Inactive")}",
                        module: "Fuel Clients",
                        type: viewModel.IsActive ? NotificationType.Success : NotificationType.Warning,
                        actionUrl: $"/FuelClients/Details/{client.Id}",
                        actionText: "View Client"
                    );
                }
                // ==================================================

                TempData["Success"] = $"Client {client.FullName} updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // POST: FuelClients/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.FuelClients.FindAsync(id);
            if (client == null)
            {
                return Json(new { success = false, message = "Client not found." });
            }

            string clientName = client.FullName;
            string clientId = client.ClientId;

            _context.FuelClients.Remove(client);
            await _context.SaveChangesAsync();

            // ===== CREATE NOTIFICATION FOR CLIENT DELETED =====
            await _notificationService.CreateNotificationAsync(
                title: $"Client Deleted: {clientId}",
                description: $"Client {clientName} has been removed from the system",
                module: "Fuel Clients",
                type: NotificationType.Warning,
                actionUrl: "/FuelClients",
                actionText: "View Clients"
            );
            // ==================================================

            return Json(new { success = true, message = $"Client {clientName} deleted successfully." });
        }

        // GET: FuelClients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.FuelClients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // Helper method to generate Client ID
        private async Task<string> GenerateClientId()
        {
            var lastClient = await _context.FuelClients
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastClient != null && !string.IsNullOrEmpty(lastClient.ClientId))
            {
                var lastNumber = lastClient.ClientId.Replace("CLI-", "");
                if (int.TryParse(lastNumber, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            return $"CLI-{nextNumber:D3}";
        }
    }
}