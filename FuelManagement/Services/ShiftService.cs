using FuelManagement.Data;
using FuelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FuelManagement.Services
{
    public class ShiftService
    {
        private readonly ApplicationDbContext _context;

        public ShiftService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Shift>> GetAllAsync()
            => await _context.Shifts.OrderByDescending(s => s.CreatedAt).ToListAsync();

        public async Task<Shift?> GetByIdAsync(int id)
            => await _context.Shifts.FindAsync(id);

        public async Task CreateAsync(Shift shift)
        {
            shift.CreatedAt = DateTime.Now;
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Shift shift)
        {
            var existing = await _context.Shifts.FindAsync(shift.Id);
            if (existing == null) return;

            existing.ShiftName = shift.ShiftName;
            existing.AssignedTo = shift.AssignedTo;
            existing.PumpNumber = shift.PumpNumber;
            existing.StartTime = shift.StartTime;
            existing.EndTime = shift.EndTime;
            existing.OpeningMeter = shift.OpeningMeter;
            existing.ClosingMeter = shift.ClosingMeter;
            existing.CashCollected = shift.CashCollected;
            existing.Status = shift.Status;
            existing.Notes = shift.Notes;

            // Only update avatar if a new one was provided
            if (!string.IsNullOrEmpty(shift.AvatarPath))
                existing.AvatarPath = shift.AvatarPath;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift != null)
            {
                _context.Shifts.Remove(shift);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CloseShiftAsync(int id, decimal closingMeter, decimal cashCollected)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift != null)
            {
                shift.EndTime = DateTime.Now;
                shift.ClosingMeter = closingMeter;
                shift.CashCollected = cashCollected;
                shift.Status = "Closed";
                await _context.SaveChangesAsync();
            }
        }
    }
}
