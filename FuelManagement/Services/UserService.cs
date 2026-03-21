using FuelManagement.Data;
using FuelManagement.Models;
using FuelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FuelManagement.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // Get All Users
        // =========================
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        // =========================
        // Get By Id
        // =========================
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // =========================
        // Get User By Email
        // =========================
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // =========================
        // Email Exists Check
        // =========================
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Users.Where(u => u.Email == email);

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        // =========================
        // Phone Exists Check - NEW
        // =========================
        public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(phone)) return false;

            var query = _context.Users.Where(u => u.Phone == phone);

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        // =========================
        // Create User - FIXED with Phone
        // =========================
        public async Task<User> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string phone,
            string role,
            string passwordHash)
        {
            var user = new User
            {
                FullName = $"{firstName} {lastName}",
                Email = email,
                Phone = phone,
                Role = role,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // =========================
        // Update User (by parameters) - FIXED with Phone
        // =========================
        public async Task<bool> UpdateUserAsync(
            int id,
            string firstName,
            string lastName,
            string email,
            string phone,
            string role,
            bool isActive)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                return false;

            user.FullName = $"{firstName} {lastName}";
            user.Email = email;
            user.Phone = phone;
            user.Role = role;
            user.IsActive = isActive;

            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // Update User (by User object)
        // =========================
        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // Get User Sessions
        // =========================
        public async Task<List<ActiveSession>> GetUserSessionsAsync(int userId)
        {
            // You'll need a UserSessions table for this
            // For now, return empty list or mock data
            return await Task.FromResult(new List<ActiveSession>());
        }

        // =========================
        // Terminate User Session
        // =========================
        public async Task<bool> TerminateUserSessionAsync(int userId, int sessionId)
        {
            // Implement when you have UserSessions table
            return await Task.FromResult(true);
        }

        // =========================
        // Delete
        // =========================
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // =========================
        // Toggle Status
        // =========================
        public async Task<bool> ToggleUserStatusAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                return false;

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}