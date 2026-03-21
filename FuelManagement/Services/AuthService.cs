using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FuelManagement.Services
{
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
        private const string SessionKeyUser = "UserEmail";
        private const string SessionKeyAuth = "IsAuthenticated";
        private const string SessionKeyTheme = "UserTheme";
        private const string SessionKeyUserId = "UserId";
        private const string SessionKeyUserRole = "UserRole"; // ADD THIS

        public AuthService(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        public async Task<bool> ValidateLogin(string username, string password)
        {
            // First check if it's the demo account (for testing)
            if (username == "Ayub" && password == "123")
            {
                return true;
            }

            // Otherwise check against database
            var user = await _userService.GetUserByEmailAsync(username);
            if (user == null)
                return false;

            // For debugging - remove after fixing
            Console.WriteLine($"Input password: {password}");
            Console.WriteLine($"Stored hash: {user.PasswordHash}");
            Console.WriteLine($"Calculated hash: {HashPassword(password)}");

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task SignIn(string username, bool rememberMe)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString(SessionKeyUser, username);
                session.SetString(SessionKeyAuth, "true");

                // Get user from database to store ID and ROLE
                var user = await _userService.GetUserByEmailAsync(username);
                if (user != null)
                {
                    session.SetInt32(SessionKeyUserId, user.Id);
                    session.SetString(SessionKeyUserRole, user.Role); // ADD THIS - Store user role
                }

                // Store the current theme in session
                var theme = GetCurrentTheme();
                session.SetString(SessionKeyTheme, theme);

                // Set session timeout based on remember me
                if (rememberMe)
                {
                    session.SetInt32("Timeout", 60 * 24 * 7); // 7 days in minutes
                }
                else
                {
                    session.SetInt32("Timeout", 20); // 20 minutes
                }
            }
        }

        public void SignOut()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(SessionKeyUser);
                session.Remove(SessionKeyAuth);
                session.Remove(SessionKeyTheme);
                session.Remove(SessionKeyUserId);
                session.Remove(SessionKeyUserRole); // ADD THIS
                session.Clear();
            }

            // Clear theme cookie on logout
            var response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Delete(".FuelMS.Theme");
        }

        public bool IsAuthenticated()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return false;

            var auth = session.GetString(SessionKeyAuth);
            return auth == "true";
        }

        public string? GetCurrentUser()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetString(SessionKeyUser);
        }

        public int? GetCurrentUserId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetInt32(SessionKeyUserId);
        }

        // ADD THIS METHOD - Get current user role
        public string? GetCurrentUserRole()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetString(SessionKeyUserRole);
        }

        // ADD THIS METHOD - Check if current user is Admin
        public bool IsAdmin()
        {
            var role = GetCurrentUserRole();
            return role == "Admin";
        }

        public string? GetUserTheme()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetString(SessionKeyTheme);
        }

        private string GetCurrentTheme()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var themeCookie = request?.Cookies[".FuelMS.Theme"];

            if (!string.IsNullOrEmpty(themeCookie))
            {
                return themeCookie;
            }

            var localStorage = request?.Cookies["theme"]; // Fallback
            return !string.IsNullOrEmpty(localStorage) ? localStorage : "light";
        }

        // Password hashing using SHA256
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Verify password
        public bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }

        // Helper method to update password (call this once to fix the hash)
        public string GetCorrectHash(string password)
        {
            return HashPassword(password);
        }
    }
}