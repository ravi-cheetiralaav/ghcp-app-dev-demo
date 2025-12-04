using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ExpenseTrackerContext _context;

        public ExpenseService(ExpenseTrackerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Expense>> GetUserExpensesAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && !e.IsDeleted)
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Expense?> GetExpenseByIdAsync(int id, int userId)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId && !e.IsDeleted);
        }

        public async Task<Expense> CreateExpenseAsync(Expense expense)
        {
            expense.CreatedDate = DateTime.UtcNow;
            expense.CreatedBy = expense.UserId;
            
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            
            return expense;
        }

        public async Task<Expense> UpdateExpenseAsync(Expense expense)
        {
            var existingExpense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == expense.Id && e.UserId == expense.UserId);
            
            if (existingExpense == null)
                throw new InvalidOperationException("Expense not found or access denied");

            existingExpense.Description = expense.Description;
            existingExpense.Amount = expense.Amount;
            existingExpense.Currency = expense.Currency;
            existingExpense.CategoryId = expense.CategoryId;
            existingExpense.ExpenseDate = expense.ExpenseDate;
            existingExpense.IsTaxDeductible = expense.IsTaxDeductible;
            existingExpense.IsRecurring = expense.IsRecurring;
            existingExpense.RecurringFrequency = expense.RecurringFrequency;
            existingExpense.RecurringEndDate = expense.RecurringEndDate;
            existingExpense.ModifiedDate = DateTime.UtcNow;
            existingExpense.ModifiedBy = expense.UserId;

            await _context.SaveChangesAsync();
            return existingExpense;
        }

        public async Task<bool> DeleteExpenseAsync(int id, int userId)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            
            if (expense == null) return false;

            expense.IsDeleted = true;
            expense.ModifiedDate = DateTime.UtcNow;
            expense.ModifiedBy = userId;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Expense>> SearchExpensesAsync(int userId, string? searchTerm, 
            int? categoryId, string? currency, bool? isTaxDeductible, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && !e.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Description.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(currency))
            {
                query = query.Where(e => e.Currency == currency);
            }

            if (isTaxDeductible.HasValue)
            {
                query = query.Where(e => e.IsTaxDeductible == isTaxDeductible.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= toDate.Value.Date);
            }

            return await query
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(int userId, int categoryId)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.CategoryId == categoryId && !e.IsDeleted)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetRecurringExpensesAsync(int userId)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.IsRecurring && !e.IsDeleted)
                .OrderBy(e => e.Description)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalExpensesAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId && !e.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value.Date);

            var expenses = await query.ToListAsync();
            return expenses.Sum(e => e.Amount);
        }

        public async Task<Dictionary<string, decimal>> GetExpensesByCurrencyAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId && !e.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value.Date);

            var expenses = await query.ToListAsync();
            return expenses
                .GroupBy(e => e.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && !e.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value.Date);

            var expenses = await query.ToListAsync();
            return expenses
                .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }
    }
}