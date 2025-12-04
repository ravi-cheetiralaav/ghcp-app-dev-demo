using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<Expense>> GetUserExpensesAsync(int userId, int page = 1, int pageSize = 10);
        Task<Expense?> GetExpenseByIdAsync(int id, int userId);
        Task<Expense> CreateExpenseAsync(Expense expense);
        Task<Expense> UpdateExpenseAsync(Expense expense);
        Task<bool> DeleteExpenseAsync(int id, int userId);
        Task<IEnumerable<Expense>> SearchExpensesAsync(int userId, string? searchTerm, int? categoryId, 
            string? currency, bool? isTaxDeductible, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(int userId, int categoryId);
        Task<IEnumerable<Expense>> GetRecurringExpensesAsync(int userId);
        Task<decimal> GetTotalExpensesAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<string, decimal>> GetExpensesByCurrencyAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}