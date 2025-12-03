using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.ViewModels
{
    public class ExpenseViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        [Display(Name = "Tax Deductible")]
        public bool IsTaxDeductible { get; set; }

        [Display(Name = "Recurring Expense")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Recurring Frequency")]
        public RecurringFrequency? RecurringFrequency { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? RecurringEndDate { get; set; }

        // Navigation properties for display
        public Category? Category { get; set; }
        public string CategoryName => Category?.Name ?? "Uncategorized";
    public string CurrencySymbol => CurrencyHelper.GetSymbol(Currency);
}

// Report ViewModels
public class ReportFilterViewModel
{
    [Display(Name = "Report Type")]
    public ReportType ReportType { get; set; } = ReportType.Annual;

    [Display(Name = "Year")]
    public int Year { get; set; } = DateTime.Now.Year;

    [Display(Name = "Month")]
    public int? Month { get; set; }

    [Display(Name = "Use Financial Year (July-June)")]
    public bool UseFinancialYear { get; set; } = true;

    [Display(Name = "Show amounts in AUD")]
    public bool ShowAudConversion { get; set; } = false;

    [Display(Name = "From Date")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Display(Name = "To Date")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    public List<int> AvailableYears { get; set; } = new();
    public List<MonthOption> AvailableMonths { get; set; } = new()
    {
        new() { Value = 1, Name = "January" },
        new() { Value = 2, Name = "February" },
        new() { Value = 3, Name = "March" },
        new() { Value = 4, Name = "April" },
        new() { Value = 5, Name = "May" },
        new() { Value = 6, Name = "June" },
        new() { Value = 7, Name = "July" },
        new() { Value = 8, Name = "August" },
        new() { Value = 9, Name = "September" },
        new() { Value = 10, Name = "October" },
        new() { Value = 11, Name = "November" },
        new() { Value = 12, Name = "December" }
    };
}

public class MonthOption
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public MonthlyReportData? MonthlyReport { get; set; }
    public AnnualReportData? AnnualReport { get; set; }
    public CustomReportData? CustomReport { get; set; }
    public string ReportTitle { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
}

public enum ReportType
{
    Monthly,
    Annual,
    Custom
}    public class ExpenseSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Currency")]
        public string? Currency { get; set; }

        [Display(Name = "Tax Deductible")]
        public bool? IsTaxDeductible { get; set; }

        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        public IEnumerable<ExpenseViewModel> Expenses { get; set; } = new List<ExpenseViewModel>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string[] Currencies { get; set; } = Enum.GetNames<ExpenseCurrency>();
    }

    public class DashboardViewModel
    {
        public decimal TotalExpensesThisMonth { get; set; }
        public decimal TotalExpensesThisYear { get; set; }
        public int TotalTransactionsThisMonth { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, decimal> CurrencyBreakdown { get; set; } = new();
        public IEnumerable<ExpenseViewModel> RecentExpenses { get; set; } = new List<ExpenseViewModel>();
        public IEnumerable<ExpenseViewModel> RecurringExpenses { get; set; } = new List<ExpenseViewModel>();
        public string UserDefaultCurrency { get; set; } = "USD";
    }
}