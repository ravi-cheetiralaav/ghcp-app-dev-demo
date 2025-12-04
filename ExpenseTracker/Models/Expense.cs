namespace ExpenseTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? CategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime ExpenseDate { get; set; } = DateTime.Today;
        public bool IsTaxDeductible { get; set; } = false;
        public bool IsRecurring { get; set; } = false;
        public RecurringFrequency? RecurringFrequency { get; set; }
        public DateTime? RecurringEndDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Category? Category { get; set; }
    }
}