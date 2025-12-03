using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public interface IReportingService
    {
        Task<MonthlyReportData> GetMonthlyReportAsync(int userId, int year, int month);
        Task<AnnualReportData> GetAnnualReportAsync(int userId, int year, bool isFinancialYear = false);
        Task<CustomReportData> GetCustomReportAsync(int userId, DateTime fromDate, DateTime toDate);
    }

    public class MonthlyReportData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, decimal> CurrencyBreakdown { get; set; } = new();
        public decimal TaxDeductibleAmount { get; set; }
        public decimal RecurringAmount { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class AnnualReportData
    {
        public int Year { get; set; }
        public bool IsFinancialYear { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<string, decimal> MonthlyBreakdown { get; set; } = new();
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, decimal> CurrencyBreakdown { get; set; } = new();
        public decimal TaxDeductibleAmount { get; set; }
        public decimal RecurringAmount { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class CustomReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, decimal> CurrencyBreakdown { get; set; } = new();
        public Dictionary<string, decimal> DailyBreakdown { get; set; } = new();
        public decimal TaxDeductibleAmount { get; set; }
        public decimal RecurringAmount { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }
}