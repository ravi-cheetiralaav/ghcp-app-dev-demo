namespace ExpenseTracker.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; } // For UI icons (Font Awesome classes)
        public string? ColorClass { get; set; } // For UI colors (Bootstrap color classes)
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Properties
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}