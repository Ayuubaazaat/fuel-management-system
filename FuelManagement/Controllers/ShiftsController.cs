using FuelManagement.Models;
using FuelManagement.Services;
using FuelManagement.Services.Interfaces; // [ADDED] Required to inject INotificationService
using Microsoft.AspNetCore.Mvc;

namespace FuelManagement.Controllers
{
    public class ShiftsController : Controller
    {
        private readonly ShiftService _shiftService;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationService _notificationService; // [ADDED] Notification service field

        // [ADDED] INotificationService injected via constructor so all actions can fire notifications
        public ShiftsController(ShiftService shiftService, IWebHostEnvironment env, INotificationService notificationService)
        {
            _shiftService = shiftService;
            _env = env;
            _notificationService = notificationService; // [ADDED] Assign injected service to field
        }

        public async Task<IActionResult> Index()
        {
            var shifts = await _shiftService.GetAllAsync();
            return View(shifts);
        }

        public IActionResult Create()
        {
            var shift = new Shift { StartTime = DateTime.Now };
            return View(shift);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shift shift)
        {
            if (!ModelState.IsValid) return View(shift);

            // Save avatar if uploaded
            if (shift.AvatarFile != null && shift.AvatarFile.Length > 0)
                shift.AvatarPath = await SaveAvatarAsync(shift.AvatarFile);

            await _shiftService.CreateAsync(shift);

            // [ADDED] Fire shift-started notification after successfully saving the new shift
            // [ADDED] Action URL points to the Details page using the newly assigned shift Id
            await _notificationService.CreateShiftStartedNotification(
                shift.Id,                                      // [ADDED] Shift primary key (assigned after CreateAsync)
                shift.ShiftName,                               // [ADDED] Human-readable shift name
                shift.AssignedTo,                              // [ADDED] Employee name assigned to this shift
                shift.PumpNumber,                              // [ADDED] Pump this shift operates on
                Url.Action("Details", "Shifts", new { id = shift.Id })! // [ADDED] Link to Details view
            );

            TempData["Success"] = "Shift started successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            if (shift == null) return NotFound();
            return View(shift);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Shift shift)
        {
            if (!ModelState.IsValid) return View(shift);

            // Keep existing avatar if no new file uploaded
            if (shift.AvatarFile != null && shift.AvatarFile.Length > 0)
            {
                // Delete old avatar file if exists
                var existing = await _shiftService.GetByIdAsync(shift.Id);
                if (existing != null && !string.IsNullOrEmpty(existing.AvatarPath))
                    DeleteAvatarFile(existing.AvatarPath);

                shift.AvatarPath = await SaveAvatarAsync(shift.AvatarFile);
            }
            else
            {
                // Preserve existing AvatarPath
                var existing = await _shiftService.GetByIdAsync(shift.Id);
                if (existing != null)
                    shift.AvatarPath = existing.AvatarPath;
            }

            await _shiftService.UpdateAsync(shift);

            // [ADDED] Fire shift-updated notification after the record has been saved successfully
            await _notificationService.CreateShiftUpdatedNotification(
                shift.Id,                                      // [ADDED] Shift primary key for reference
                shift.ShiftName,                               // [ADDED] Shift name for the notification title
                shift.AssignedTo,                              // [ADDED] Employee name shown in description
                Url.Action("Details", "Shifts", new { id = shift.Id })! // [ADDED] Link to Details view
            );

            TempData["Success"] = "Shift updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            if (shift == null) return NotFound();
            return View(shift);
        }

        [HttpPost]
        public async Task<IActionResult> CloseShift(int id, decimal closingMeter, decimal cashCollected)
        {
            // [ADDED] Fetch the shift BEFORE closing so we have ShiftName and AssignedTo for the notification
            var shift = await _shiftService.GetByIdAsync(id); // [ADDED] Load shift details prior to close

            await _shiftService.CloseShiftAsync(id, closingMeter, cashCollected);

            // [ADDED] Fire shift-closed notification only if the shift record was found
            if (shift != null) // [ADDED] Guard against null in case the id was invalid
            {
                await _notificationService.CreateShiftClosedNotification(
                    id,                                                // [ADDED] Shift Id
                    shift.ShiftName,                                   // [ADDED] Shift name for title/description
                    shift.AssignedTo,                                  // [ADDED] Employee name for description
                    closingMeter,                                      // [ADDED] Closing meter value passed from form
                    cashCollected,                                     // [ADDED] Cash collected value passed from form
                    Url.Action("Details", "Shifts", new { id })!       // [ADDED] Link to the now-closed shift details
                );
            }

            TempData["Success"] = "Shift closed successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // [ADDED] Fetch shift BEFORE deleting so we can pass ShiftName and AssignedTo to the notification
            var shift = await _shiftService.GetByIdAsync(id); // [ADDED] Load shift before deletion

            // Delete avatar file from disk before removing record
            if (shift != null && !string.IsNullOrEmpty(shift.AvatarPath))
                DeleteAvatarFile(shift.AvatarPath);

            await _shiftService.DeleteAsync(id);

            // [ADDED] Fire shift-deleted notification only if we successfully loaded the shift before deletion
            if (shift != null) // [ADDED] Guard so we don't fire a notification for a non-existent record
            {
                await _notificationService.CreateShiftDeletedNotification(
                    id,                                                    // [ADDED] Shift Id (record is now deleted)
                    shift.ShiftName,                                       // [ADDED] Name captured before deletion
                    shift.AssignedTo,                                      // [ADDED] Employee name captured before deletion
                    Url.Action("Index", "Shifts")!                         // [ADDED] Points to Index since record is gone
                );
            }

            TempData["Success"] = "Shift deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<string> SaveAvatarAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "shifts");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"shift_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/shifts/{fileName}";
        }

        private void DeleteAvatarFile(string avatarPath)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, avatarPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch { /* ignore file delete errors */ }
        }
    }
}