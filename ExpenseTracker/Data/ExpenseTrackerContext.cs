using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;

namespace ExpenseTracker.Data
{
    public class ExpenseTrackerContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ExpenseTrackerContext(DbContextOptions<ExpenseTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration (Identity is already configured by base class)
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Expense Configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ExpenseDate });
                entity.HasIndex(e => e.CategoryId);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("USD");
                entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Expenses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Expenses)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Configure enum properties
                entity.Property(e => e.RecurringFrequency)
                      .HasConversion<string>();
            });

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconClass).HasMaxLength(50);
                entity.Property(e => e.ColorClass).HasMaxLength(50);
            });

            // UserSettings Configuration
            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.DefaultCurrency).HasMaxLength(3).HasDefaultValue("USD");
                entity.Property(e => e.Theme).HasMaxLength(20).HasDefaultValue("Light");
                entity.Property(e => e.DateFormat).HasMaxLength(20).HasDefaultValue("MM/dd/yyyy");
                entity.Property(e => e.NumberFormat).HasMaxLength(20).HasDefaultValue("en-US");
                entity.Property(e => e.TimeZone).HasMaxLength(100).HasDefaultValue("UTC");
                
                entity.HasOne(s => s.User)
                      .WithOne(u => u.Settings)
                      .HasForeignKey<UserSettings>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            var categories = new[]
            {
                new Category { Id = 1, Name = "Food & Dining", Description = "Restaurants, groceries, and food delivery", IconClass = "fas fa-utensils", ColorClass = "text-success", CreatedDate = DateTime.UtcNow },
                new Category { Id = 2, Name = "Transportation", Description = "Gas, parking, public transit, rideshare", IconClass = "fas fa-car", ColorClass = "text-primary", CreatedDate = DateTime.UtcNow },
                new Category { Id = 3, Name = "Shopping", Description = "Clothing, electronics, general purchases", IconClass = "fas fa-shopping-cart", ColorClass = "text-warning", CreatedDate = DateTime.UtcNow },
                new Category { Id = 4, Name = "Entertainment", Description = "Movies, games, subscriptions, hobbies", IconClass = "fas fa-gamepad", ColorClass = "text-info", CreatedDate = DateTime.UtcNow },
                new Category { Id = 5, Name = "Bills & Utilities", Description = "Electric, water, internet, phone", IconClass = "fas fa-receipt", ColorClass = "text-danger", CreatedDate = DateTime.UtcNow },
                new Category { Id = 6, Name = "Healthcare", Description = "Medical expenses, pharmacy, insurance", IconClass = "fas fa-heartbeat", ColorClass = "text-secondary", CreatedDate = DateTime.UtcNow },
                new Category { Id = 7, Name = "Education", Description = "Books, courses, training, tuition", IconClass = "fas fa-graduation-cap", ColorClass = "text-dark", CreatedDate = DateTime.UtcNow },
                new Category { Id = 8, Name = "Travel", Description = "Hotels, flights, vacation expenses", IconClass = "fas fa-plane", ColorClass = "text-primary", CreatedDate = DateTime.UtcNow },
                new Category { Id = 9, Name = "Home & Garden", Description = "Repairs, maintenance, furniture, tools", IconClass = "fas fa-home", ColorClass = "text-success", CreatedDate = DateTime.UtcNow },
                new Category { Id = 10, Name = "Business", Description = "Office supplies, professional services", IconClass = "fas fa-briefcase", ColorClass = "text-warning", CreatedDate = DateTime.UtcNow },
                new Category { Id = 11, Name = "Personal Care", Description = "Haircuts, cosmetics, gym membership", IconClass = "fas fa-user", ColorClass = "text-info", CreatedDate = DateTime.UtcNow },
                new Category { Id = 12, Name = "Miscellaneous", Description = "Other expenses not fitting other categories", IconClass = "fas fa-tags", ColorClass = "text-muted", CreatedDate = DateTime.UtcNow }
            };

            modelBuilder.Entity<Category>().HasData(categories);
        }
    }
}