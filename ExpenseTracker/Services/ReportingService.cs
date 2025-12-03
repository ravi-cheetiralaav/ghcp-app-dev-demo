using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using System.Globalization;

namespace ExpenseTracker.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ExpenseTrackerContext _context;
        private readonly IExchangeRateService _exchangeRateService;

        public ReportingService(ExpenseTrackerContext context, IExchangeRateService exchangeRateService)
        {
            _context = context;
            _exchangeRateService = exchangeRateService;
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

            // Calculate proper amounts considering only tax deductible expenses and recurring frequencies
            var totalTaxDeductibleAmount = CalculateTotalTaxDeductibleAmount(expenses, 1);
            var totalRecurringAmount = CalculateRecurringAmountForPeriod(expenses, 1);
            var taxDeductibleExpenses = expenses.Where(e => e.IsTaxDeductible).ToList();

            return new MonthlyReportData
            {
                Year = year,
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                TotalAmount = totalTaxDeductibleAmount, // Only tax deductible expenses
                CategoryBreakdown = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => CalculateCategoryTotalForPeriod(g.ToList(), 1)),
                CurrencyBreakdown = expenses
                    .GroupBy(e => e.Currency)
                    .ToDictionary(g => g.Key, g => CalculateCategoryTotalForPeriod(g.ToList(), 1)),
                TaxDeductibleAmount = totalTaxDeductibleAmount,
                RecurringAmount = totalRecurringAmount,
                TotalTransactions = taxDeductibleExpenses.Count
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

            // Calculate proper amounts considering only tax deductible expenses and recurring frequencies for the year
            var totalTaxDeductibleAmount = CalculateTotalTaxDeductibleAmount(expenses, 12);
            var totalRecurringAmount = CalculateRecurringAmountForPeriod(expenses, 12);
            var taxDeductibleExpenses = expenses.Where(e => e.IsTaxDeductible).ToList();

            // Monthly breakdown should also consider only tax deductible expenses
            var monthlyBreakdown = expenses
                .Where(e => e.IsTaxDeductible)
                .GroupBy(e => e.ExpenseDate.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => CalculateCategoryTotalForPeriod(g.ToList(), 1)); // Monthly calculation

            return new AnnualReportData
            {
                Year = year,
                IsFinancialYear = isFinancialYear,
                TotalAmount = totalTaxDeductibleAmount, // Only tax deductible expenses
                MonthlyBreakdown = monthlyBreakdown,
                CategoryBreakdown = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => CalculateCategoryTotalForPeriod(g.ToList(), 12)),
                CurrencyBreakdown = expenses
                    .GroupBy(e => e.Currency)
                    .ToDictionary(g => g.Key, g => CalculateCategoryTotalForPeriod(g.ToList(), 12)),
                TaxDeductibleAmount = totalTaxDeductibleAmount,
                RecurringAmount = totalRecurringAmount,
                TotalTransactions = taxDeductibleExpenses.Count
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

            // Calculate number of months in the period for recurring calculations
            var monthsInPeriod = CalculateMonthsInPeriod(fromDate, toDate);
            var totalTaxDeductibleAmount = CalculateTotalTaxDeductibleAmount(expenses, monthsInPeriod);
            var totalRecurringAmount = CalculateRecurringAmountForPeriod(expenses, monthsInPeriod);
            var taxDeductibleExpenses = expenses.Where(e => e.IsTaxDeductible).ToList();

            return new CustomReportData
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalAmount = totalTaxDeductibleAmount, // Only tax deductible expenses
                CategoryBreakdown = expenses
                    .GroupBy(e => e.Category?.Name ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => CalculateCategoryTotalForPeriod(g.ToList(), monthsInPeriod)),
                CurrencyBreakdown = expenses
                    .GroupBy(e => e.Currency)
                    .ToDictionary(g => g.Key, g => CalculateCategoryTotalForPeriod(g.ToList(), monthsInPeriod)),
                DailyBreakdown = expenses
                    .Where(e => e.IsTaxDeductible)
                    .GroupBy(e => e.ExpenseDate.ToString("yyyy-MM-dd"))
                    .ToDictionary(g => g.Key, g => g.Sum(e => GetAdjustedExpenseAmount(e, 1.0m / 30.0m))), // Daily approximation
                TaxDeductibleAmount = totalTaxDeductibleAmount,
                RecurringAmount = totalRecurringAmount,
                TotalTransactions = taxDeductibleExpenses.Count,
                AverageTransactionAmount = taxDeductibleExpenses.Count > 0 ? totalTaxDeductibleAmount / taxDeductibleExpenses.Count : 0
            };
        }

        private decimal CalculateTotalTaxDeductibleAmount(List<Expense> expenses, decimal monthsInPeriod)
        {
            decimal total = 0;
            
            foreach (var expense in expenses.Where(e => e.IsTaxDeductible))
            {
                total += GetAdjustedExpenseAmount(expense, monthsInPeriod);
            }
            
            return total;
        }

        private decimal CalculateRecurringAmountForPeriod(List<Expense> expenses, decimal monthsInPeriod)
        {
            return expenses
                .Where(e => e.IsRecurring && e.IsTaxDeductible)
                .Sum(e => GetAdjustedExpenseAmount(e, monthsInPeriod));
        }

        private decimal CalculateCategoryTotalForPeriod(List<Expense> expenses, decimal monthsInPeriod)
        {
            return expenses
                .Where(e => e.IsTaxDeductible)
                .Sum(e => GetAdjustedExpenseAmount(e, monthsInPeriod));
        }

        private decimal GetAdjustedExpenseAmount(Expense expense, decimal periodMultiplier)
        {
            if (!expense.IsRecurring)
            {
                // For non-recurring expenses, return the original amount
                return expense.Amount;
            }

            // For recurring expenses, calculate based on frequency
            var frequencyMultiplier = GetFrequencyMultiplier(expense.RecurringFrequency);
            
            // Calculate the amount for the specified period
            // For example: $50 monthly expense for 12 months = $50 * 12 = $600
            return expense.Amount * frequencyMultiplier * periodMultiplier;
        }

        private decimal GetFrequencyMultiplier(RecurringFrequency? frequency)
        {
            return frequency switch
            {
                RecurringFrequency.Weekly => 52.0m / 12.0m,      // ~4.33 weeks per month
                RecurringFrequency.Fortnightly => 26.0m / 12.0m, // ~2.17 fortnights per month
                RecurringFrequency.Monthly => 1.0m,              // 1 month per month
                RecurringFrequency.Quarterly => 1.0m / 3.0m,     // 0.33 quarters per month
                RecurringFrequency.Annually => 1.0m / 12.0m,     // 0.083 years per month
                _ => 1.0m // Default to monthly if not specified
            };
        }

        private decimal CalculateMonthsInPeriod(DateTime fromDate, DateTime toDate)
        {
            var totalDays = (toDate - fromDate).Days + 1;
            return (decimal)totalDays / 30.44m; // Average days per month
        }

        // AUD Conversion Methods
        public async Task<MonthlyReportData> GetMonthlyReportWithAudAsync(int userId, int year, int month)
        {
            var report = await GetMonthlyReportAsync(userId, year, month);
            await ConvertReportToAudAsync(report);
            return report;
        }

        public async Task<AnnualReportData> GetAnnualReportWithAudAsync(int userId, int year, bool isFinancialYear = false)
        {
            var report = await GetAnnualReportAsync(userId, year, isFinancialYear);
            await ConvertReportToAudAsync(report);
            return report;
        }

        public async Task<CustomReportData> GetCustomReportWithAudAsync(int userId, DateTime fromDate, DateTime toDate)
        {
            var report = await GetCustomReportAsync(userId, fromDate, toDate);
            await ConvertReportToAudAsync(report);
            return report;
        }

        private async Task ConvertReportToAudAsync(MonthlyReportData report)
        {
            try
            {
                report.ShowAudConversion = true;
                
                // Get exchange rates
                var rates = await _exchangeRateService.GetLatestRatesAsync();
                if (rates?.IsSuccess == true)
                {
                    report.ExchangeRateTimestamp = rates.LastUpdated;
                    report.HasAudConversion = true;

                    // Convert main amounts (assuming they are already in mixed currencies, we need original data)
                    report.TotalAmountAud = await ConvertCurrencyBreakdownToAud(report.CurrencyBreakdown);
                    report.TaxDeductibleAmountAud = await EstimateAudAmount(report.TaxDeductibleAmount, report.CurrencyBreakdown, report.TotalAmount);
                    report.RecurringAmountAud = await EstimateAudAmount(report.RecurringAmount, report.CurrencyBreakdown, report.TotalAmount);

                    // Convert category breakdown (this is more complex as we need original currency data)
                    foreach (var category in report.CategoryBreakdown)
                    {
                        // For now, estimate based on proportion - ideally we'd have currency data per category
                        var proportion = report.TotalAmount > 0 ? category.Value / report.TotalAmount : 0;
                        report.CategoryBreakdownAud[category.Key] = Math.Round(report.TotalAmountAud * proportion, 2);
                    }

                    // Set the new AudConversionData structure
                    report.AudConversionData = new AudConversionData
                    {
                        TotalAudAmount = report.TotalAmountAud,
                        CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        ExchangeRates = rates.ConversionRates.ToDictionary(k => k.Key, v => v.Value),
                        LastUpdated = rates.LastUpdated,
                        CategoryBreakdownAud = report.CategoryBreakdownAud.ToDictionary(k => k.Key, v => v.Value),
                        IsSuccess = true,
                        ErrorMessage = rates.ErrorMessage
                    };

                    if (!string.IsNullOrEmpty(rates.ErrorMessage))
                    {
                        report.ConversionErrorMessage = rates.ErrorMessage;
                    }
                }
                else
                {
                    report.HasAudConversion = false;
                    report.ConversionErrorMessage = rates?.ErrorMessage ?? "Currency conversion service unavailable";
                    report.ExchangeRateTimestamp = DateTime.UtcNow;
                    
                    // Set AUD values to original values as fallback
                    report.TotalAmountAud = report.TotalAmount;
                    report.TaxDeductibleAmountAud = report.TaxDeductibleAmount;
                    report.RecurringAmountAud = report.RecurringAmount;
                    report.CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value);
                    
                    // Set fallback AudConversionData
                    report.AudConversionData = new AudConversionData
                    {
                        TotalAudAmount = report.TotalAmount,
                        CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        ExchangeRates = new Dictionary<string, decimal>(),
                        LastUpdated = null,
                        CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        IsSuccess = false,
                        ErrorMessage = rates?.ErrorMessage ?? "Currency conversion service unavailable"
                    };
                }
            }
            catch (Exception ex)
            {
                report.ShowAudConversion = true; // Still show conversion section with error
                report.HasAudConversion = false;
                report.ConversionErrorMessage = $"Currency conversion failed: {ex.Message}";
                report.ExchangeRateTimestamp = DateTime.UtcNow;
                
                // Fallback to original values
                report.TotalAmountAud = report.TotalAmount;
                report.TaxDeductibleAmountAud = report.TaxDeductibleAmount;
                report.RecurringAmountAud = report.RecurringAmount;
                report.CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value);
                
                // Set error AudConversionData
                report.AudConversionData = new AudConversionData
                {
                    TotalAudAmount = report.TotalAmount,
                    CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                    ExchangeRates = new Dictionary<string, decimal>(),
                    LastUpdated = null,
                    CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value),
                    IsSuccess = false,
                    ErrorMessage = $"Currency conversion failed: {ex.Message}"
                };
            }
        }

        private async Task ConvertReportToAudAsync(AnnualReportData report)
        {
            try
            {
                report.ShowAudConversion = true;
                
                var rates = await _exchangeRateService.GetLatestRatesAsync();
                if (rates?.IsSuccess == true)
                {
                    report.ExchangeRateTimestamp = rates.LastUpdated;
                    report.HasAudConversion = true;

                    report.TotalAmountAud = await ConvertCurrencyBreakdownToAud(report.CurrencyBreakdown);
                    report.TaxDeductibleAmountAud = await EstimateAudAmount(report.TaxDeductibleAmount, report.CurrencyBreakdown, report.TotalAmount);
                    report.RecurringAmountAud = await EstimateAudAmount(report.RecurringAmount, report.CurrencyBreakdown, report.TotalAmount);

                    // Convert monthly breakdown
                    foreach (var month in report.MonthlyBreakdown)
                    {
                        var proportion = report.TotalAmount > 0 ? month.Value / report.TotalAmount : 0;
                        report.MonthlyBreakdownAud[month.Key] = Math.Round(report.TotalAmountAud * proportion, 2);
                    }

                    // Convert category breakdown
                    foreach (var category in report.CategoryBreakdown)
                    {
                        var proportion = report.TotalAmount > 0 ? category.Value / report.TotalAmount : 0;
                        report.CategoryBreakdownAud[category.Key] = Math.Round(report.TotalAmountAud * proportion, 2);
                    }

                    // Set the new AudConversionData structure
                    report.AudConversionData = new AudConversionData
                    {
                        TotalAudAmount = report.TotalAmountAud,
                        CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        ExchangeRates = rates.ConversionRates.ToDictionary(k => k.Key, v => v.Value),
                        LastUpdated = rates.LastUpdated,
                        CategoryBreakdownAud = report.CategoryBreakdownAud.ToDictionary(k => k.Key, v => v.Value),
                        IsSuccess = true,
                        ErrorMessage = rates.ErrorMessage
                    };

                    if (!string.IsNullOrEmpty(rates.ErrorMessage))
                    {
                        report.ConversionErrorMessage = rates.ErrorMessage;
                    }
                }
                else
                {
                    report.HasAudConversion = false;
                    report.ConversionErrorMessage = rates?.ErrorMessage ?? "Currency conversion service unavailable";
                    report.ExchangeRateTimestamp = DateTime.UtcNow;
                    
                    // Fallback values
                    report.TotalAmountAud = report.TotalAmount;
                    report.TaxDeductibleAmountAud = report.TaxDeductibleAmount;
                    report.RecurringAmountAud = report.RecurringAmount;
                    report.MonthlyBreakdownAud = report.MonthlyBreakdown.ToDictionary(k => k.Key, v => v.Value);
                    report.CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value);
                    
                    // Set fallback AudConversionData
                    report.AudConversionData = new AudConversionData
                    {
                        TotalAudAmount = report.TotalAmount,
                        CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        ExchangeRates = new Dictionary<string, decimal>(),
                        LastUpdated = null,
                        CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        IsSuccess = false,
                        ErrorMessage = rates?.ErrorMessage ?? "Currency conversion service unavailable"
                    };
                }
            }
            catch (Exception ex)
            {
                report.ShowAudConversion = true; // Still show conversion section with error
                report.HasAudConversion = false;
                report.ConversionErrorMessage = $"Currency conversion failed: {ex.Message}";
                report.ExchangeRateTimestamp = DateTime.UtcNow;
                
                // Fallback values
                report.TotalAmountAud = report.TotalAmount;
                report.TaxDeductibleAmountAud = report.TaxDeductibleAmount;
                report.RecurringAmountAud = report.RecurringAmount;
                report.MonthlyBreakdownAud = report.MonthlyBreakdown.ToDictionary(k => k.Key, v => v.Value);
                report.CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value);
                
                // Set error AudConversionData
                report.AudConversionData = new AudConversionData
                {
                    TotalAudAmount = report.TotalAmount,
                    CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                    ExchangeRates = new Dictionary<string, decimal>(),
                    LastUpdated = null,
                    CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value),
                    IsSuccess = false,
                    ErrorMessage = $"Currency conversion failed: {ex.Message}"
                };
            }
        }

        private async Task ConvertReportToAudAsync(CustomReportData report)
        {
            try
            {
                report.ShowAudConversion = true;
                
                var rates = await _exchangeRateService.GetLatestRatesAsync();
                if (rates?.IsSuccess == true)
                {
                    report.ExchangeRateTimestamp = rates.LastUpdated;
                    report.HasAudConversion = true;

                    report.TotalAmountAud = await ConvertCurrencyBreakdownToAud(report.CurrencyBreakdown);
                    report.TaxDeductibleAmountAud = await EstimateAudAmount(report.TaxDeductibleAmount, report.CurrencyBreakdown, report.TotalAmount);
                    report.RecurringAmountAud = await EstimateAudAmount(report.RecurringAmount, report.CurrencyBreakdown, report.TotalAmount);
                    report.AverageTransactionAmountAud = report.TotalTransactions > 0 ? report.TotalAmountAud / report.TotalTransactions : 0;

                    // Convert daily breakdown
                    foreach (var day in report.DailyBreakdown)
                    {
                        var proportion = report.TotalAmount > 0 ? day.Value / report.TotalAmount : 0;
                        report.DailyBreakdownAud[day.Key] = Math.Round(report.TotalAmountAud * proportion, 2);
                    }

                    // Convert category breakdown
                    foreach (var category in report.CategoryBreakdown)
                    {
                        var proportion = report.TotalAmount > 0 ? category.Value / report.TotalAmount : 0;
                        report.CategoryBreakdownAud[category.Key] = Math.Round(report.TotalAmountAud * proportion, 2);
                    }

                    // Set the new AudConversionData structure
                    report.AudConversionData = new AudConversionData
                    {
                        TotalAudAmount = report.TotalAmountAud,
                        CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        ExchangeRates = rates.ConversionRates.ToDictionary(k => k.Key, v => v.Value),
                        LastUpdated = rates.LastUpdated,
                        CategoryBreakdownAud = report.CategoryBreakdownAud.ToDictionary(k => k.Key, v => v.Value),
                        IsSuccess = true,
                        ErrorMessage = rates.ErrorMessage
                    };

                    if (!string.IsNullOrEmpty(rates.ErrorMessage))
                    {
                        report.ConversionErrorMessage = rates.ErrorMessage;
                    }
                }
                else
                {
                    report.HasAudConversion = false;
                    report.ConversionErrorMessage = rates?.ErrorMessage ?? "Currency conversion service unavailable";
                    report.ExchangeRateTimestamp = DateTime.UtcNow;
                    
                    // Fallback values
                    report.TotalAmountAud = report.TotalAmount;
                    report.TaxDeductibleAmountAud = report.TaxDeductibleAmount;
                    report.RecurringAmountAud = report.RecurringAmount;
                    report.AverageTransactionAmountAud = report.AverageTransactionAmount;
                    report.DailyBreakdownAud = report.DailyBreakdown.ToDictionary(k => k.Key, v => v.Value);
                    report.CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value);
                    
                    // Set fallback AudConversionData
                    report.AudConversionData = new AudConversionData
                    {
                        TotalAudAmount = report.TotalAmount,
                        CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        ExchangeRates = new Dictionary<string, decimal>(),
                        LastUpdated = null,
                        CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value),
                        IsSuccess = false,
                        ErrorMessage = rates?.ErrorMessage ?? "Currency conversion service unavailable"
                    };
                }
            }
            catch (Exception ex)
            {
                report.ShowAudConversion = true; // Still show conversion section with error
                report.HasAudConversion = false;
                report.ConversionErrorMessage = $"Currency conversion failed: {ex.Message}";
                report.ExchangeRateTimestamp = DateTime.UtcNow;
                
                // Fallback values
                report.TotalAmountAud = report.TotalAmount;
                report.TaxDeductibleAmountAud = report.TaxDeductibleAmount;
                report.RecurringAmountAud = report.RecurringAmount;
                report.AverageTransactionAmountAud = report.AverageTransactionAmount;
                report.DailyBreakdownAud = report.DailyBreakdown.ToDictionary(k => k.Key, v => v.Value);
                report.CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value);
                
                // Set error AudConversionData
                report.AudConversionData = new AudConversionData
                {
                    TotalAudAmount = report.TotalAmount,
                    CurrencyBreakdown = report.CurrencyBreakdown.ToDictionary(k => k.Key, v => v.Value),
                    ExchangeRates = new Dictionary<string, decimal>(),
                    LastUpdated = null,
                    CategoryBreakdownAud = report.CategoryBreakdown.ToDictionary(k => k.Key, v => v.Value),
                    IsSuccess = false,
                    ErrorMessage = $"Currency conversion failed: {ex.Message}"
                };
            }
        }

        private async Task<decimal> ConvertCurrencyBreakdownToAud(Dictionary<string, decimal> currencyBreakdown)
        {
            decimal totalAud = 0;
            foreach (var currency in currencyBreakdown)
            {
                var audAmount = await _exchangeRateService.ConvertToAudAsync(currency.Value, currency.Key);
                totalAud += audAmount;
            }
            return Math.Round(totalAud, 2);
        }

        private async Task<decimal> EstimateAudAmount(decimal amount, Dictionary<string, decimal> currencyBreakdown, decimal totalAmount)
        {
            if (totalAmount <= 0) return 0;
            
            var totalAud = await ConvertCurrencyBreakdownToAud(currencyBreakdown);
            var proportion = amount / totalAmount;
            return Math.Round(totalAud * proportion, 2);
        }
    }
}
