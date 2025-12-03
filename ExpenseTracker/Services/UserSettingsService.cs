using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly ExpenseTrackerContext _context;

        public UserSettingsService(ExpenseTrackerContext context)
        {
            _context = context;
        }

        public async Task<UserSettings?> GetUserSettingsAsync(int userId)
        {
            return await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<UserSettings> CreateOrUpdateUserSettingsAsync(UserSettings settings)
        {
            var existingSettings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == settings.UserId);

            if (existingSettings == null)
            {
                settings.CreatedDate = DateTime.UtcNow;
                _context.UserSettings.Add(settings);
            }
            else
            {
                existingSettings.DefaultCurrency = settings.DefaultCurrency;
                existingSettings.Theme = settings.Theme;
                existingSettings.DateFormat = settings.DateFormat;
                existingSettings.NumberFormat = settings.NumberFormat;
                existingSettings.TimeZone = settings.TimeZone;
                existingSettings.ItemsPerPage = settings.ItemsPerPage;
                existingSettings.ModifiedDate = DateTime.UtcNow;
                settings = existingSettings;
            }

            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<string> GetUserDefaultCurrencyAsync(int userId)
        {
            var settings = await GetUserSettingsAsync(userId);
            return settings?.DefaultCurrency ?? "USD";
        }

        public async Task<string> GetUserThemeAsync(int userId)
        {
            var settings = await GetUserSettingsAsync(userId);
            return settings?.Theme ?? "Light";
        }

        public async Task<string> GetUserDateFormatAsync(int userId)
        {
            var settings = await GetUserSettingsAsync(userId);
            return settings?.DateFormat ?? "MM/dd/yyyy";
        }
    }
}