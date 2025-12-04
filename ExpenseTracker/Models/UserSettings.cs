namespace ExpenseTracker.Models
{
    public class UserSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DefaultCurrency { get; set; } = "USD";
        public string Theme { get; set; } = "Light";
        public string DateFormat { get; set; } = "MM/dd/yyyy";
        public string NumberFormat { get; set; } = "en-US";
        public string TimeZone { get; set; } = "UTC";
        public int ItemsPerPage { get; set; } = 10;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Properties
        public virtual User User { get; set; } = null!;
    }
}