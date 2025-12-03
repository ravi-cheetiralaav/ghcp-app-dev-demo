using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using System.Globalization;

namespace ExpenseTracker.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ExpenseTrackerContext _context;

        public ReportingService(ExpenseTrackerContext context)
        {
            _context = context;
        }

        public async Task<MonthlyReportData> GetMonthlyReportAsync(int userId, int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && 
                           e.ExpenseDate >= firstDay && 
                           e.ExpenseDate <= lastDay && 
                           !e.IsDeleted)
                .ToListAsync();

            return new MonthlyReportData
            {
                Year = year,
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                TotalAmount = expenses.Sum(e => e.Amount),
                CategoryBreakdown = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                CurrencyBreakdown = expenses
                    .GroupBy(e => e.Currency)
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                TaxDeductibleAmount = expenses
                    .Where(e => e.IsTaxDeductible)
                    .Sum(e => e.Amount),
                RecurringAmount = expenses
                    .Where(e => e.IsRecurring)
                    .Sum(e => e.Amount),
                TotalTransactions = expenses.Count
            };
        }

        public async Task<AnnualReportData> GetAnnualReportAsync(int userId, int year, bool isFinancialYear = false)
        {
            DateTime startDate, endDate;
            
            if (isFinancialYear)
            {
                // Financial year: July to June
                startDate = new DateTime(year, 7, 1);
                endDate = new DateTime(year + 1, 6, 30);
            }
            else
            {
                // Calendar year
                startDate = new DateTime(year, 1, 1);
                endDate = new DateTime(year, 12, 31);
            }

            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && 
                           e.ExpenseDate >= startDate && 
                           e.ExpenseDate <= endDate && 
                           !e.IsDeleted)
                .ToListAsync();

            var monthlyBreakdown = expenses
                .GroupBy(e => e.ExpenseDate.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            return new AnnualReportData
            {
                Year = year,
                IsFinancialYear = isFinancialYear,
                TotalAmount = expenses.Sum(e => e.Amount),
                MonthlyBreakdown = monthlyBreakdown,
                CategoryBreakdown = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                CurrencyBreakdown = expenses
                    .GroupBy(e => e.Currency)
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                TaxDeductibleAmount = expenses
                    .Where(e => e.IsTaxDeductible)
                    .Sum(e => e.Amount),
                RecurringAmount = expenses
                    .Where(e => e.IsRecurring)
                    .Sum(e => e.Amount),
                TotalTransactions = expenses.Count
            };
        }

        public async Task<CustomReportData> GetCustomReportAsync(int userId, DateTime fromDate, DateTime toDate)
        {
            var expenses = await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && 
                           e.ExpenseDate >= fromDate.Date && 
                           e.ExpenseDate <= toDate.Date && 
                           !e.IsDeleted)
                .ToListAsync();

            var totalAmount = expenses.Sum(e => e.Amount);
            var totalTransactions = expenses.Count;

            return new CustomReportData
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalAmount = totalAmount,
                CategoryBreakdown = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                CurrencyBreakdown = expenses
                    .GroupBy(e => e.Currency)
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                DailyBreakdown = expenses
                    .GroupBy(e => e.ExpenseDate.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount)),
                TaxDeductibleAmount = expenses
                    .Where(e => e.IsTaxDeductible)
                    .Sum(e => e.Amount),
                RecurringAmount = expenses
                    .Where(e => e.IsRecurring)
                    .Sum(e => e.Amount),
                TotalTransactions = totalTransactions,
                AverageTransactionAmount = totalTransactions > 0 ? totalAmount / totalTransactions : 0
            };
        }
    }
}