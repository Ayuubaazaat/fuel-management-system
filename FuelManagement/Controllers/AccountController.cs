using FuelManagement.Models;
using FuelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuelManagement.Models.ViewModels;
using System;
using System.IO;

namespace FuelManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(AuthService authService, UserService userService, IWebHostEnvironment environment)
        {
            _authService = authService;
            _userService = userService;
            _environment = environment;
        }


        // Helper to get device name
        private string GetDeviceName()
        {
            // Try to get from User-Agent header
            var userAgent = Request.Headers["User-Agent"].ToString();

            if (!string.IsNullOrEmpty(userAgent))
            {
                if (userAgent.Contains("Windows NT"))
                    return "Windows PC";
                if (userAgent.Contains("Macintosh"))
                    return "Mac";
                if (userAgent.Contains("iPhone"))
                    return "iPhone";
                if (userAgent.Contains("iPad"))
                    return "iPad";
                if (userAgent.Contains("Android"))
                    return "Android Device";
                if (userAgent.Contains("Linux"))
                    return "Linux PC";
            }

            // Fallback to computer name or generic
            return Environment.MachineName ?? "Unknown Device";
        }

        // ===========================
        // SIGN IN (REGISTRATION) - GET
        // ===========================
        [HttpGet]
        public IActionResult SignIn()
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        // ===========================
        // SIGN IN (REGISTRATION) - POST
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(RegisterViewModel model, IFormFile? Avatar)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if terms are accepted
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError("AcceptTerms", "You must accept the terms and conditions");
                return View(model);
            }

            try
            {
                // Check if email already exists
                if (await _userService.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View(model);
                }

                // Check if phone already exists (if provided)
                if (!string.IsNullOrEmpty(model.Phone) && await _userService.PhoneExistsAsync(model.Phone))
                {
                    ModelState.AddModelError("Phone", "This phone number is already registered");
                    return View(model);
                }

                // Hash the password
                string passwordHash = _authService.HashPassword(model.Password);

                // Create the user (default role is "User")
                var user = await _userService.CreateUserAsync(
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Phone ?? "",
                    "User", // Default role for self-registration
                    passwordHash
                );

                // Handle avatar upload if provided
                if (Avatar != null && Avatar.Length > 0)
                {
                    await HandleAvatarUpload(user.Id, Avatar);
                }

                // Show success message and redirect to login
                TempData["SuccessMessage"] = "Account created successfully! Please sign in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating your account. Please try again.");
                return View(model);
            }
        }

        // ===========================
        // Helper method for avatar upload
        // ===========================
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
                    user.ProfileImageUrl = $"/uploads/avatars/{fileName}";
                    await _userService.UpdateUserAsync(user);
                }
            }
            catch
            {
                // Silently fail - don't prevent registration if avatar fails
            }
        }

        // ===========================
        // SWITCH ACCOUNT - GET
        // ===========================
        [HttpGet]
        public async Task<IActionResult> SwitchAccount()
        {
            var users = await _userService.GetAllUsersAsync();

            var viewModel = users.Select(u => new SwitchAccountViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                ProfileImageUrl = u.ProfileImageUrl ?? "/images/avatars/default.jpg",
                Role = u.Role
            }).ToList();

            return View(viewModel);
        }

        // ===========================
        // SWITCH ACCOUNT LOGIN - POST
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchAccountLogin(int userId, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Password is required.";
                return RedirectToAction(nameof(SwitchAccount));
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(SwitchAccount));
            }

            // Verify password using AuthService
            if (!_authService.VerifyPassword(password, user.PasswordHash))
            {
                TempData["Error"] = "Invalid password.";
                return RedirectToAction(nameof(SwitchAccount));
            }

            // Sign in the user
            await _authService.SignIn(user.Email, rememberMe: false);

            // Create claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Store in session for backward compatibility
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            // Update login stats
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            user.LastLoginDevice = GetDeviceName();
            user.LoginCount += 1;
            await _userService.UpdateUserAsync(user);

            TempData["Success"] = $"Welcome back, {user.FullName}!";
            return RedirectToAction("Index", "Home");
        }

        // ===========================
        // LOGIN - GET
        // ===========================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }
        // ===========================
        // LOGIN - POST  ← CHANGED
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Step 1 — check if email exists
            var user = await _userService.GetUserByEmailAsync(model.Username);

            // Step 2 — email not found
            if (user == null)
            {
                // Check if password is also wrong (we can't verify without a user)
                // Both wrong — email doesn't exist, password can't be verified
                ModelState.AddModelError(string.Empty, "Incorrect Email and Password.");
                return View(model);
            }

            // Step 3 — email found, now verify password
            bool passwordCorrect = await _authService.ValidateLogin(user.Email, model.Password);

            if (!passwordCorrect)
            {
                // Email is correct but password is wrong
                ModelState.AddModelError(string.Empty, "Password is incorrect.");
                return View(model);
            }

            // Step 4 — both correct, check if active
            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account is deactivated. Please contact administrator.");
                return View(model);
            }

            // Step 5 — sign in
            await _authService.SignIn(user.Email, model.RememberMe);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,           user.FullName),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            user.LastLoginDevice = GetDeviceName();
            user.LoginCount += 1;
            await _userService.UpdateUserAsync(user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }
        // ===========================
        // FORGOT PASSWORD - GET
        // ===========================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        // ===========================
        // FORGOT PASSWORD - POST (email lookup)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Please enter your email.");
                return View(model);
            }

            // Look up the user using the existing UserService
            var user = await _userService.GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                model.NotFound = true;
                return View(model);
            }

            model.UserFound = true;
            model.FoundUserFullName = user.FullName;
            model.FoundUserEmail = user.Email;
            model.FoundUserProfileImage = user.ProfileImageUrl;
            return View(model);
        }

        // ===========================
        // RESET PASSWORD - POST
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ResetError"] = "Passwords do not match.";
                return RedirectToAction("ForgotPassword");
            }

            // Look up user by email using existing UserService
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                TempData["ResetError"] = "Account not found.";
                return RedirectToAction("ForgotPassword");
            }

            // Hash the new password using the same AuthService.HashPassword
            // already used throughout this controller
            user.PasswordHash = _authService.HashPassword(newPassword);
            await _userService.UpdateUserAsync(user);

            TempData["SuccessMessage"] = "Password reset successfully! Please sign in with your new password.";
            return RedirectToAction("Login");
        }


        // ===========================
        // LOGOUT
        // ===========================
        [HttpGet]
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutConfirmed()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ===========================
        // ACCESS DENIED
        // ===========================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}