using Microsoft.AspNetCore.Mvc;
using FuelManagement.Models;
using FuelManagement.Services;
using FuelManagement.Services.Interfaces;
using FuelManagement.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FuelManagement.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _environment;

        public UsersController(
            UserService userService,
            AuthService authService,
            INotificationService notificationService,
            IWebHostEnvironment environment)
        {
            _userService = userService;
            _authService = authService;
            _notificationService = notificationService;
            _environment = environment;
        }

        // =========================
        // GET: /Users
        // =========================
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();

            var viewModels = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                FirstName = u.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = string.Join(" ", u.FullName.Split(' ').Skip(1)),
                Email = u.Email,
                Phone = u.Phone ?? "Not provided",
                Role = u.Role,
                Status = u.IsActive ? "Active" : "Inactive",
                IsActive = u.IsActive,
                JoinDate = u.CreatedAt,
                LastLogin = u.LastLoginAt,
                LastLoginIp = u.LastLoginIp,
                LoginDevice = u.LastLoginDevice,
                TotalLogins = u.LoginCount,
                IsEmailVerified = u.EmailConfirmed,
                ProfileImageUrl = u.ProfileImageUrl
            }).ToList();

            return View(viewModels);
        }

        // =========================
        // GET: /Users/Details/5
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            var viewModel = new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = string.Join(" ", user.FullName.Split(' ').Skip(1)),
                Email = user.Email,
                Phone = user.Phone ?? "Not provided",
                Role = user.Role,
                Status = user.IsActive ? "Active" : "Inactive",
                IsActive = user.IsActive,
                JoinDate = user.CreatedAt,
                LastLogin = user.LastLoginAt,
                LastLoginIp = user.LastLoginIp,
                LoginDevice = user.LastLoginDevice,
                TotalLogins = user.LoginCount,
                IsEmailVerified = user.EmailConfirmed,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return View(viewModel);
        }

        // =========================
        // GET: /Users/Create
        // =========================
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        // =========================
        // POST: /Users/Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model, IFormFile? avatar)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _userService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Email address is already registered");
                return View(model);
            }
            // ===== ADD PHONE VALIDATION HERE =====
            if (!string.IsNullOrEmpty(model.Phone) && await _userService.PhoneExistsAsync(model.Phone))
            {
                ModelState.AddModelError("Phone", "This phone number is already registered");
                return View(model);
            }
            // =====================================
            try
            {
                string passwordHash = _authService.HashPassword(model.Password);

                var user = await _userService.CreateUserAsync(
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Phone,
                    model.Role,
                    passwordHash
                );

                // Handle avatar upload if provided
                if (avatar != null && avatar.Length > 0)
                {
                    await HandleAvatarUpload(user.Id, avatar);

                    // Refresh user to get updated ProfileImageUrl
                    user = await _userService.GetUserByIdAsync(user.Id);
                }

                // ===== NOTIFICATION: New User Created =====
                await _notificationService.CreateUserCreatedNotification(
                    userName: $"{model.FirstName} {model.LastName}",
                    userRole: model.Role,
                    actionUrl: $"/Users/Details/{user.Id}"
                );
                // ==========================================

                TempData["Success"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
                return View(model);
            }
        }

        // =========================
        // GET: /Users/Edit/5
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                FirstName = user.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = string.Join(" ", user.FullName.Split(' ').Skip(1)),
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return View(model);
        }

        // =========================
        // POST: /Users/Edit
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model, IFormFile? avatar)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _userService.EmailExistsAsync(model.Email, model.Id))
            {
                ModelState.AddModelError("Email", "Email address is already registered to another user");
                return View(model);
            }

            // ===== ADD PHONE VALIDATION HERE =====
            if (!string.IsNullOrEmpty(model.Phone) && await _userService.PhoneExistsAsync(model.Phone, model.Id))
            {
                ModelState.AddModelError("Phone", "This phone number is already registered to another user");
                return View(model);
            }
            // =====================================
            try
            {
                // Get current user to compare role and status
                var currentUser = await _userService.GetUserByIdAsync(model.Id);
                var oldRole = currentUser?.Role ?? "";
                var oldStatus = currentUser?.IsActive == true ? "Active" : "Inactive";
                var oldPhone = currentUser?.Phone;
                var oldName = currentUser?.FullName;

                var success = await _userService.UpdateUserAsync(
                    model.Id,
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Phone,
                    model.Role,
                    model.IsActive
                );

                if (!success)
                    return NotFound();

                // Handle avatar upload if provided
                bool avatarUpdated = false;
                if (avatar != null && avatar.Length > 0)
                {
                    await HandleAvatarUpload(model.Id, avatar);
                    avatarUpdated = true;
                }

                // Get the updated user to refresh data
                var updatedUser = await _userService.GetUserByIdAsync(model.Id);
                var newStatus = model.IsActive ? "Active" : "Inactive";
                var fullName = $"{model.FirstName} {model.LastName}";

                // ===== NOTIFICATION: User Role Updated =====
                if (oldRole != model.Role)
                {
                    await _notificationService.CreateUserRoleUpdatedNotification(
                        userName: fullName,
                        oldRole: oldRole,
                        newRole: model.Role,
                        actionUrl: $"/Users/Details/{model.Id}"
                    );
                }

                // ===== NOTIFICATION: User Status Changed =====
                if (oldStatus != newStatus)
                {
                    await _notificationService.CreateNotificationAsync(
                        title: $"User Status Changed: {fullName}",
                        description: $"User {fullName} status changed from {oldStatus} to {newStatus}",
                        module: "Users",
                        type: model.IsActive ? NotificationType.Success : NotificationType.Warning,
                        actionUrl: $"/Users/Details/{model.Id}",
                        actionText: "View User"
                    );
                }

                // ===== NOTIFICATION: User Profile Updated =====
                // This triggers when ONLY profile info changes (name, phone, email) but role/status stay same
                if (oldRole == model.Role && oldStatus == newStatus && !avatarUpdated)
                {
                    await _notificationService.CreateNotificationAsync(
                        title: $"User Profile Updated",
                        description: $"{fullName}'s profile has been updated",
                        module: "Users",
                        type: NotificationType.Info,
                        actionUrl: $"/Users/Details/{model.Id}",
                        actionText: "View Profile"
                    );
                }

                // ===== NOTIFICATION: Avatar Updated =====
                if (avatarUpdated && oldRole == model.Role && oldStatus == newStatus)
                {
                    await _notificationService.CreateNotificationAsync(
                        title: $"Profile Photo Updated",
                        description: $"{fullName}'s profile photo has been updated",
                        module: "Users",
                        type: NotificationType.Success,
                        actionUrl: $"/Users/Details/{model.Id}",
                        actionText: "View Profile"
                    );
                }
                // ================================================

                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while updating the user.");
                return View(model);
            }
        }
        // =========================
        // POST: /Users/Delete/5
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var userName = user.FullName;
                var userEmail = user.Email;

                await _userService.DeleteUserAsync(id);

                // ===== NOTIFICATION: User Deleted =====
                await _notificationService.CreateNotificationAsync(
                    title: "User Deleted",
                    description: $"User {userName} ({userEmail}) has been removed from the system",
                    module: "Users",
                    type: NotificationType.Warning,
                    actionUrl: "/Users",
                    actionText: "View Users"
                );
                // ======================================

                TempData["Success"] = "User deleted successfully!";
            }
            catch
            {
                TempData["Error"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // POST: /Users/ToggleStatus/5
        // =========================
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return Json(new { success = false });

                var oldStatus = user.IsActive ? "Active" : "Inactive";

                var success = await _userService.ToggleUserStatusAsync(id);

                if (!success)
                    return Json(new { success = false });

                var updatedUser = await _userService.GetUserByIdAsync(id);
                var newStatus = updatedUser?.IsActive == true ? "Active" : "Inactive";

                // ===== NOTIFICATION: User Status Changed =====
                await _notificationService.CreateNotificationAsync(
                    title: $"User Status Changed: {updatedUser?.FullName}",
                    description: $"User {updatedUser?.FullName} status changed from {oldStatus} to {newStatus}",
                    module: "Users",
                    type: newStatus == "Active" ? NotificationType.Success : NotificationType.Warning,
                    actionUrl: $"/Users/Details/{id}",
                    actionText: "View User"
                );
                // ============================================

                return Json(new
                {
                    success = true,
                    status = newStatus
                });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        // =========================
        // POST: /Users/UploadAvatar/{id}
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return Json(new { success = false, message = "No file uploaded" });

            await HandleAvatarUpload(id, avatar);

            var user = await _userService.GetUserByIdAsync(id);

            return Json(new
            {
                success = true,
                imageUrl = user?.ProfileImageUrl
            });
        }

        // =========================
        // Helper method for avatar upload
        // =========================
        private async Task HandleAvatarUpload(int userId, IFormFile avatar)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return;

            if (avatar.Length > 5 * 1024 * 1024) // 5MB limit
                return;

            try
            {
                var fileName = $"avatar_{userId}_{Guid.NewGuid()}{extension}";
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    // Delete old avatar if exists
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != "/images/avatars/default.jpg")
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.ProfileImageUrl = $"/uploads/avatars/{fileName}";
                    await _userService.UpdateUserAsync(user);
                }
            }
            catch
            {
                // Silently fail - don't prevent user operations if avatar fails
            }
        }
    }
}