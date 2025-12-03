using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Models
{
    public class User : IdentityUser<int>
    {
        public override int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int LoginAttempts { get; set; } = 0;
        
        // Navigation Properties
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual UserSettings? Settings { get; set; }
    }
}