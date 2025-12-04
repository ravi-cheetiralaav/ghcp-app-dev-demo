# ExpenseTracker Web Application

A modern ASP.NET Core web application for expense tracking, modernized from a legacy VB.NET Windows Forms desktop application.

## 🌟 Features

- **Multi-User Support** - Secure user registration and authentication
- **Expense Management** - Add, edit, delete, and categorize expenses
- **Pre-defined Categories** - 12 built-in expense categories with icons
- **Multi-Currency Support** - Track expenses in different currencies (USD, EUR, GBP, etc.)
- **Recurring Expenses** - Set up recurring transactions
- **Dashboard & Analytics** - Visual charts and expense summaries
- **Responsive Design** - Works on desktop, tablet, and mobile devices
- **User Settings** - Customizable preferences and settings

## 🏗️ Architecture

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5.3, Font Awesome, Chart.js
- **Architecture Pattern**: Clean Architecture with Service Layer

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git (for cloning the repository)

## 🚀 Getting Started

### Clone the Repository

```bash
git clone <repository-url>
cd ghcp-app-dev-demo
```

### Build and Run

1. **Navigate to the project directory:**
   ```bash
   cd ExpenseTracker
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the application:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Access the application:**
   - Open your browser and navigate to: `http://localhost:5171`
   - The database will be automatically created on first run
   - Categories will be seeded automatically

### Development Build

For development with hot reload:

```bash
dotnet watch run
```

### Clean Build

To perform a clean build:

```bash
dotnet clean
dotnet build
```

## 🗄️ Database

The application uses SQLite with Entity Framework Core:

- **Database File**: `expensetracker.db` (created automatically)
- **Migrations**: Applied automatically on startup
- **Seeded Data**: 12 expense categories are pre-loaded

### Database Schema

- **AspNetUsers** - User accounts and profiles
- **Categories** - Expense categories (Food, Transportation, etc.)
- **Expenses** - User expense records
- **UserSettings** - User preferences and settings

## 🎯 Usage Guide

### First Time Setup

1. **Register a New Account:**
   - Navigate to `/Account/Register`
   - Create your account with email and password
   - User settings will be created automatically

2. **Login:**
   - Navigate to `/Account/Login`
   - Use your credentials to access the dashboard

3. **Dashboard:**
   - View expense summaries and charts
   - Quick access to recent expenses
   - Currency-wise and category-wise breakdowns

### Managing Expenses

- **Add Expenses**: Use the expense management interface
- **Categories**: Choose from 12 pre-defined categories
- **Recurring**: Set up recurring transactions
- **Multi-Currency**: Track expenses in different currencies

## 🧪 Testing

The application includes comprehensive error handling and SQLite compatibility fixes:

- All decimal aggregation operations use client-side processing
- Entity Framework queries are optimized for SQLite
- User isolation ensures data security

## 📁 Project Structure

```
ExpenseTracker/
├── Controllers/          # MVC Controllers
│   ├── AccountController.cs
│   ├── HomeController.cs
│   └── ExpenseController.cs
├── Models/              # Data Models
│   ├── User.cs
│   ├── Expense.cs
│   ├── Category.cs
│   └── UserSettings.cs
├── Services/            # Business Logic
│   ├── ExpenseService.cs
│   ├── CategoryService.cs
│   ├── UserSettingsService.cs
│   └── ReportingService.cs
├── Data/                # Entity Framework Context
│   └── ExpenseTrackerContext.cs
├── Views/               # Razor Views
│   ├── Shared/
│   ├── Home/
│   └── Account/
├── wwwroot/             # Static Files
│   ├── css/
│   ├── js/
│   └── lib/
├── Program.cs           # Application Entry Point
└── appsettings.json     # Configuration
```

## ⚙️ Configuration

### Connection String

SQLite connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=expensetracker.db"
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Set to `Development` for development mode
- `ASPNETCORE_URLS` - Override default URLs (default: `http://localhost:5171`)

## 🔧 Troubleshooting

### Common Issues

1. **Port Already in Use:**
   - Change the port in `Properties/launchSettings.json`
   - Or use: `dotnet run --urls "http://localhost:5172"`

2. **Database Issues:**
   - Delete `expensetracker.db` to reset the database
   - Run `dotnet ef database update` to apply migrations

3. **Build Errors:**
   - Ensure .NET 8.0 SDK is installed
   - Run `dotnet clean` then `dotnet build`

### SQLite Decimal Aggregation

The application handles SQLite's decimal aggregation limitations by:
- Using `ToListAsync()` before Sum operations
- Performing aggregations client-side
- Maintaining data type integrity

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 License

This project is part of a modernization demo and is for educational/demonstration purposes.

## 🆘 Support

For issues and questions:
1. Check the troubleshooting section
2. Review the demo guide
3. Check application logs in the terminal output

---

**Note**: This application was modernized from a legacy VB.NET Windows Forms desktop application to demonstrate modern web development practices with ASP.NET Core.