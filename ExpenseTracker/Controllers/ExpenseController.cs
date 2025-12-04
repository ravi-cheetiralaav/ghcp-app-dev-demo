using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(
            IExpenseService expenseService,
            ICategoryService categoryService,
            IUserSettingsService userSettingsService,
            ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
            _userSettingsService = userSettingsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(ExpenseSearchViewModel searchModel)
        {
            var userId = GetCurrentUserId();
            var categories = await _categoryService.GetActiveCategoriesAsync();

            IEnumerable<Expense> expenses;

            if (HasSearchCriteria(searchModel))
            {
                expenses = await _expenseService.SearchExpensesAsync(
                    userId,
                    searchModel.SearchTerm,
                    searchModel.CategoryId,
                    searchModel.Currency,
                    searchModel.IsTaxDeductible,
                    searchModel.FromDate,
                    searchModel.ToDate);
            }
            else
            {
                expenses = await _expenseService.GetUserExpensesAsync(userId, searchModel.Page, searchModel.PageSize);
                
                // Get total count for pagination
                var allExpenses = await _expenseService.GetUserExpensesAsync(userId, 1, int.MaxValue);
                searchModel.TotalRecords = allExpenses.Count();
            }

            searchModel.Expenses = expenses.Select(MapToViewModel);
            searchModel.Categories = categories;

            return View(searchModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);

            if (expense == null)
            {
                return NotFound();
            }

            return View(MapToViewModel(expense));
        }

        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();
            var defaultCurrency = await _userSettingsService.GetUserDefaultCurrencyAsync(userId);
            
            var model = new ExpenseViewModel
            {
                Currency = defaultCurrency,
                ExpenseDate = DateTime.Today
            };

            await PopulateDropdowns();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                var expense = new Expense
                {
                    UserId = userId,
                    Description = model.Description,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    CategoryId = model.CategoryId,
                    ExpenseDate = model.ExpenseDate,
                    IsTaxDeductible = model.IsTaxDeductible,
                    IsRecurring = model.IsRecurring,
                    RecurringFrequency = model.IsRecurring ? model.RecurringFrequency : null,
                    RecurringEndDate = model.IsRecurring ? model.RecurringEndDate : null
                };

                await _expenseService.CreateExpenseAsync(expense);
                TempData["SuccessMessage"] = "Expense created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);

            if (expense == null)
            {
                return NotFound();
            }

            await PopulateDropdowns();
            return View(MapToViewModel(expense));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var expense = new Expense
                    {
                        Id = model.Id,
                        UserId = userId,
                        Description = model.Description,
                        Amount = model.Amount,
                        Currency = model.Currency,
                        CategoryId = model.CategoryId,
                        ExpenseDate = model.ExpenseDate,
                        IsTaxDeductible = model.IsTaxDeductible,
                        IsRecurring = model.IsRecurring,
                        RecurringFrequency = model.IsRecurring ? model.RecurringFrequency : null,
                        RecurringEndDate = model.IsRecurring ? model.RecurringEndDate : null
                    };

                    await _expenseService.UpdateExpenseAsync(expense);
                    TempData["SuccessMessage"] = "Expense updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException)
                {
                    return NotFound();
                }
            }

            await PopulateDropdowns();
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);

            if (expense == null)
            {
                return NotFound();
            }

            return View(MapToViewModel(expense));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _expenseService.DeleteExpenseAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Expense deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetRecurring()
        {
            var userId = GetCurrentUserId();
            var recurringExpenses = await _expenseService.GetRecurringExpensesAsync(userId);
            var model = recurringExpenses.Select(MapToViewModel);
            return View(model);
        }

        private async Task PopulateDropdowns()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Currencies = new SelectList(Enum.GetNames<ExpenseCurrency>());
            ViewBag.RecurringFrequencies = new SelectList(Enum.GetValues<RecurringFrequency>());
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private static bool HasSearchCriteria(ExpenseSearchViewModel searchModel)
        {
            return !string.IsNullOrWhiteSpace(searchModel.SearchTerm) ||
                   searchModel.CategoryId.HasValue ||
                   !string.IsNullOrWhiteSpace(searchModel.Currency) ||
                   searchModel.IsTaxDeductible.HasValue ||
                   searchModel.FromDate.HasValue ||
                   searchModel.ToDate.HasValue;
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
}