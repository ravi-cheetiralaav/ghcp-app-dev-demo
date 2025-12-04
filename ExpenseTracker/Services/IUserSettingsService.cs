using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public interface IUserSettingsService
    {
        Task<UserSettings?> GetUserSettingsAsync(int userId);
        Task<UserSettings> CreateOrUpdateUserSettingsAsync(UserSettings settings);
        Task<string> GetUserDefaultCurrencyAsync(int userId);
        Task<string> GetUserThemeAsync(int userId);
        Task<string> GetUserDateFormatAsync(int userId);
    }
}