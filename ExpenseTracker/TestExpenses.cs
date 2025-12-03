using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

// This is a simple test file to add sample expenses for testing
public class TestExpenses
{
    public static async Task AddSampleExpenses(ExpenseTrackerContext context, UserManager<User> userManager)
    {
        var testUser = await userManager.FindByEmailAsync("test@example.com");
        if (testUser == null) return;

        var sampleExpenses = new List<Expense>
        {
            new Expense
            {
                UserId = testUser.Id,
                CategoryId = 1, // Food & Dining
                Description = "Lunch at downtown restaurant",
                Amount = 45.50m,
                Currency = "USD",
                ExpenseDate = DateTime.Today.AddDays(-1),
                IsTaxDeductible = false,
                IsRecurring = false,
                CreatedBy = testUser.Id
            },
            new Expense
            {
                UserId = testUser.Id,
                CategoryId = 2, // Transportation
                Description = "Gas for commute",
                Amount = 65.00m,
                Currency = "USD",
                ExpenseDate = DateTime.Today.AddDays(-2),
                IsTaxDeductible = false,
                IsRecurring = false,
                CreatedBy = testUser.Id
            },
            new Expense
            {
                UserId = testUser.Id,
                CategoryId = 3, // Shopping
                Description = "Office supplies and equipment",
                Amount = 120.75m,
                Currency = "USD",
                ExpenseDate = DateTime.Today.AddDays(-3),
                IsTaxDeductible = true,
                IsRecurring = false,
                CreatedBy = testUser.Id
            },
            new Expense
            {
                UserId = testUser.Id,
                CategoryId = 5, // Bills & Utilities
                Description = "Monthly internet bill",
                Amount = 89.99m,
                Currency = "USD",
                ExpenseDate = DateTime.Today.AddDays(-5),
                IsTaxDeductible = false,
                IsRecurring = true,
                RecurringFrequency = RecurringFrequency.Monthly,
                RecurringEndDate = DateTime.Today.AddYears(1),
                CreatedBy = testUser.Id
            },
            new Expense
            {
                UserId = testUser.Id,
                CategoryId = 8, // Travel
                Description = "Business trip hotel",
                Amount = 250.00m,
                Currency = "USD",
                ExpenseDate = DateTime.Today.AddDays(-7),
                IsTaxDeductible = true,
                IsRecurring = false,
                CreatedBy = testUser.Id
            }
        };

        context.Expenses.AddRange(sampleExpenses);
        await context.SaveChangesAsync();
    }
}