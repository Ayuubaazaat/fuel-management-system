using Microsoft.AspNetCore.Mvc;
using FuelManagement.Models;
using FuelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FuelManagement.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            UserService userService,
            AuthService authService,
            IWebHostEnvironment environment)
        {
            _userService = userService;
            _authService = authService;
            _environment = environment;
        }

        // Helper to get current user ID - NO FALLBACK
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            return null;
        }

        // Helper to get initials from full name
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "AF";
            var parts = fullName.Split(' ');
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            return fullName.Length >= 2 ? fullName.Substring(0, 2).ToUpper() : fullName.ToUpper();
        }
        // Helper to get unique device count (placeholder for now)
        private async Task<int> GetUniqueDeviceCount(int userId)
        {
            // You'll need a LoginHistory table to track this
            // For now, return 1 as a placeholder
            return await Task.FromResult(1);
        }
        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
                return NotFound();

            var nameParts = user.FullName?.Split(' ') ?? new[] { "", "" };
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email ?? "",
                Phone = user.Phone ?? "",
                Bio = user.Bio ?? "",
                Role = user.Role ?? "User",
                JoinDate = user.CreatedAt,
                LastActive = user.LastLoginAt ?? user.CreatedAt,
                ProfileImageUrl = user.ProfileImageUrl ?? "/images/avatars/default.jpg",
                AccountStatus = user.IsActive ? "Active" : "Inactive",
                EmailVerified = user.EmailConfirmed,
                PhoneVerified = !string.IsNullOrEmpty(user.Phone),
                TwoFactorEnabled = user.TwoFactorEnabled,
                TotalLogins = user.LoginCount,
                LastLoginIp = user.LastLoginIp ?? "",
                LoginDevice = user.LastLoginDevice ?? "",
                DeviceCount = await GetUniqueDeviceCount(user.Id)
            };

            // Set ViewBag values for sidebar
            ViewBag.AccountStatus = model.AccountStatus;
            ViewBag.LastLoginFormatted = user.LastLoginAt.HasValue
                ? user.LastLoginAt.Value.ToString("MMM dd, yyyy 'at' h:mm tt")
                : "Never";
            ViewBag.MemberSince = user.CreatedAt.ToString("MMMM yyyy");
            ViewBag.TotalLogins = user.LoginCount.ToString("N0");

            return View(model);
        }

        // GET: /Profile/Security
        [HttpGet]
        public async Task<IActionResult> Security()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetUserByIdAsync(userId.Value);

            var model = new SecurityViewModel
            {
                TwoFactorEnabled = user?.TwoFactorEnabled ?? false,
                ActiveSessions = new List<ActiveSession>()
            };

            return View(model);
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
                return NotFound();

            var nameParts = user.FullName?.Split(' ') ?? new[] { "", "" };
            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email ?? "",
                Phone = user.Phone,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl ?? "/images/avatars/default.jpg"
            };

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the errors below.";
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return RedirectToAction("Login", "Account");

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return NotFound();

                // Update user properties
                user.FullName = $"{model.FirstName} {model.LastName}".Trim();
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Bio = model.Bio;
                user.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateUserAsync(user);

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while saving your profile.");
                TempData["Error"] = "Failed to update profile.";
                return View(model);
            }
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "User not authenticated" });

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                if (!_authService.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                    return Json(new { success = false, message = "Current password is incorrect" });

                user.PasswordHash = _authService.HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user);

                return Json(new { success = true, message = "Password changed successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred while changing password." });
            }
        }

        // POST: /Profile/ToggleTwoFactor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTwoFactor(bool enabled)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "User not authenticated" });

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                user.TwoFactorEnabled = enabled;
                user.UpdatedAt = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user);

                return Json(new { success = true, enabled = enabled });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to update two-factor authentication." });
            }
        }

        // POST: /Profile/UploadAvatar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return Json(new { success = false, message = "No file selected." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, message = "Invalid file type. Only JPG, PNG and GIF are allowed." });

            if (avatar.Length > 5 * 1024 * 1024)
                return Json(new { success = false, message = "File size exceeds 5MB limit." });

            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Json(new { success = false, message = "User not authenticated" });

                var fileName = $"avatar_{userId.Value}_{Guid.NewGuid()}{extension}";
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user != null)
                {
                    user.ProfileImageUrl = $"/uploads/avatars/{fileName}";
                    await _userService.UpdateUserAsync(user);
                }

                return Json(new { success = true, imageUrl = $"/uploads/avatars/{fileName}" });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to upload avatar." });
            }
        }

        // GET: /Profile/GetCurrentUserAvatar - Get user avatar data for layout
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserAvatar()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Json(new { success = false });

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                hasAvatar = !string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != "/images/avatars/default.jpg",
                avatarUrl = user.ProfileImageUrl,
                initials = GetInitials(user.FullName)
            });
        }
    }
}