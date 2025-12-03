using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models;

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

    public class ExpenseSearchViewModel
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