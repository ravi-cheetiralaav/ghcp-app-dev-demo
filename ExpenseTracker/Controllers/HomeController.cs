using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IExpenseService _expenseService;
    private readonly IUserSettingsService _userSettingsService;
    private readonly IReportingService _reportingService;

    public HomeController(
        ILogger<HomeController> logger,
        IExpenseService expenseService,
        IUserSettingsService userSettingsService,
        IReportingService reportingService)
    {
        _logger = logger;
        _expenseService = expenseService;
        _userSettingsService = userSettingsService;
        _reportingService = reportingService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var currentDate = DateTime.Now;
            
            var dashboard = new DashboardViewModel
            {
                TotalExpensesThisMonth = await _expenseService.GetTotalExpensesAsync(
                    userId, 
                    new DateTime(currentDate.Year, currentDate.Month, 1), 
                    currentDate),
                TotalExpensesThisYear = await _expenseService.GetTotalExpensesAsync(
                    userId, 
                    new DateTime(currentDate.Year, 1, 1), 
                    currentDate),
                CategoryBreakdown = await _expenseService.GetExpensesByCategoryAsync(
                    userId,
                    new DateTime(currentDate.Year, currentDate.Month, 1),
                    currentDate),
                CurrencyBreakdown = await _expenseService.GetExpensesByCurrencyAsync(
                    userId,
                    new DateTime(currentDate.Year, currentDate.Month, 1),
                    currentDate),
                RecentExpenses = (await _expenseService.GetUserExpensesAsync(userId, 1, 5))
                    .Select(MapToViewModel),
                RecurringExpenses = (await _expenseService.GetRecurringExpensesAsync(userId))
                    .Take(5)
                    .Select(MapToViewModel),
                UserDefaultCurrency = await _userSettingsService.GetUserDefaultCurrencyAsync(userId)
            };

            var monthlyReport = await _reportingService.GetMonthlyReportAsync(
                userId, currentDate.Year, currentDate.Month);
            dashboard.TotalTransactionsThisMonth = monthlyReport.TotalTransactions;

            return View(dashboard);
        }

        return View(new DashboardViewModel());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private static ExpenseViewModel MapToViewModel(Expense expense)
    {
        return new ExpenseViewModel
        {
            Id = expense.Id,
            Description = expense.Description,
            Amount = expense.Amount,
            Currency = expense.Currency,
            CategoryId = expense.CategoryId,
            ExpenseDate = expense.ExpenseDate,
            IsTaxDeductible = expense.IsTaxDeductible,
            IsRecurring = expense.IsRecurring,
            RecurringFrequency = expense.RecurringFrequency,
            RecurringEndDate = expense.RecurringEndDate,
            Category = expense.Category
        };
    }
}
