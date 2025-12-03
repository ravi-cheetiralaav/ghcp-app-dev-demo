# ExpenseTracker Demo Guide

This guide demonstrates how to modernize a legacy desktop application using GitHub Copilot and AI-assisted development.

## 🎯 Demo Overview

**Scenario**: Modernizing a legacy VB.NET Windows Forms expense tracking desktop application to a modern ASP.NET Core web application.

**Goal**: Transform the desktop application into a modern, multi-user web application with enhanced features and responsive design.

## 🤖 AI Scaffolding Process

### Initial Prompt Used

The following prompt was used to scaffold the entire application using GitHub Copilot:

```
Scaffold a new C#.Net web app using the proposal document to modernise a expense tracker desktop app
```

**Supporting Document**: `docs/ExpenseTracker_Modernization_Proposal.md` - Contains the modernization requirements and specifications.

### AI-Generated Components

The AI assistant successfully generated:

1. **Project Structure** - Complete ASP.NET Core MVC application
2. **Database Models** - Entity Framework models with relationships
3. **Authentication** - ASP.NET Core Identity implementation
4. **Business Logic** - Service layer with dependency injection
5. **User Interface** - Bootstrap-based responsive views
6. **Data Access** - Repository pattern with Entity Framework
7. **Configuration** - Complete application setup and middleware



1. **Project Structure Tour**
   ```
   ExpenseTracker/
   ├── Models/              # Entity models
   ├── Controllers/         # MVC controllers
   ├── Services/           # Business logic layer
   ├── Data/               # Entity Framework context
   ├── Views/              # Razor views
   └── wwwroot/            # Static assets
   ```

2. **Key Components Generated**
   - **Models**: User, Expense, Category, UserSettings with proper relationships
   - **Services**: ExpenseService, CategoryService, UserSettingsService, ReportingService
   - **Controllers**: HomeController, AccountController, ExpenseController
   - **Views**: Responsive Bootstrap layouts with modern UI

3. **Modern Features Added**
   - Multi-user support with authentication
   - Responsive web design
   - RESTful API structure
   - Cloud-ready architecture



## 🔍 Technical Deep Dive

### Database Design

```sql
-- Auto-generated schema
- AspNetUsers (Identity users)
- Categories (12 pre-seeded categories)
- Expenses (with foreign keys and indexes)
- UserSettings (user preferences)
```



## ⚡ Quick Start for Demo

```bash
# Clone and run in under 2 minutes
git clone <repository-url>
cd ghcp-app-dev-demo/ExpenseTracker
dotnet run

# Access at http://localhost:5171
# Register → Login → Start using the modern expense tracker
```

---
