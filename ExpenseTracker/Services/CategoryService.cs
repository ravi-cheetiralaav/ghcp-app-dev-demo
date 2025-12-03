using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ExpenseTrackerContext _context;

        public CategoryService(ExpenseTrackerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.CreatedDate = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == category.Id);
            
            if (existingCategory == null)
                throw new InvalidOperationException("Category not found");

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.IconClass = category.IconClass;
            existingCategory.ColorClass = category.ColorClass;
            existingCategory.IsActive = category.IsActive;
            existingCategory.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category == null) return false;

            // Check if category has associated expenses
            var hasExpenses = await _context.Expenses
                .AnyAsync(e => e.CategoryId == id && !e.IsDeleted);
            
            if (hasExpenses)
            {
                // Soft delete - deactivate instead of removing
                category.IsActive = false;
                category.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}