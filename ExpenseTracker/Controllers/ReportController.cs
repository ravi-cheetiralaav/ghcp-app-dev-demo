using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using System.Globalization;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportingService _reportingService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IReportingService reportingService,
            UserManager<User> userManager,
            ILogger<ReportController> logger)
        {
            _reportingService = reportingService;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var currentYear = DateTime.Now.Year;
            var filter = new ReportFilterViewModel
            {
                Year = currentYear,
                UseFinancialYear = true,
                AvailableYears = GetAvailableYears()
            };

            var viewModel = new ReportViewModel
            {
                Filter = filter
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportFilterViewModel filter)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                filter.AvailableYears = GetAvailableYears();
                
                var viewModel = new ReportViewModel
                {
                    Filter = filter
                };

                switch (filter.ReportType)
                {
                    case ReportType.Monthly:
                        if (filter.Month.HasValue)
                        {
                            if (filter.ShowAudConversion)
                            {
                                viewModel.MonthlyReport = await _reportingService.GetMonthlyReportWithAudAsync(
                                    user.Id, filter.Year, filter.Month.Value);
                            }
                            else
                            {
                                viewModel.MonthlyReport = await _reportingService.GetMonthlyReportAsync(
                                    user.Id, filter.Year, filter.Month.Value);
                            }
                            
                            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(filter.Month.Value);
                            viewModel.ReportTitle = $"Monthly Report - {monthName} {filter.Year}";
                            viewModel.DateRange = $"{monthName} {filter.Year}";
                        }
                        break;

                    case ReportType.Annual:
                        if (filter.ShowAudConversion)
                        {
                            viewModel.AnnualReport = await _reportingService.GetAnnualReportWithAudAsync(
                                user.Id, filter.Year, filter.UseFinancialYear);
                        }
                        else
                        {
                            viewModel.AnnualReport = await _reportingService.GetAnnualReportAsync(
                                user.Id, filter.Year, filter.UseFinancialYear);
                        }
                        
                        if (filter.UseFinancialYear)
                        {
                            viewModel.ReportTitle = $"Financial Year Report - FY{filter.Year}/{filter.Year + 1}";
                            viewModel.DateRange = $"1 July {filter.Year} - 30 June {filter.Year + 1}";
                        }
                        else
                        {
                            viewModel.ReportTitle = $"Annual Report - {filter.Year}";
                            viewModel.DateRange = $"1 January {filter.Year} - 31 December {filter.Year}";
                        }
                        break;

                    case ReportType.Custom:
                        if (filter.FromDate.HasValue && filter.ToDate.HasValue)
                        {
                            if (filter.ShowAudConversion)
                            {
                                viewModel.CustomReport = await _reportingService.GetCustomReportWithAudAsync(
                                    user.Id, filter.FromDate.Value, filter.ToDate.Value);
                            }
                            else
                            {
                                viewModel.CustomReport = await _reportingService.GetCustomReportAsync(
                                    user.Id, filter.FromDate.Value, filter.ToDate.Value);
                            }
                            
                            viewModel.ReportTitle = "Custom Period Report";
                            viewModel.DateRange = $"{filter.FromDate.Value:dd MMM yyyy} - {filter.ToDate.Value:dd MMM yyyy}";
                        }
                        break;
                }

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "An error occurred while generating the report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReport(ReportType reportType, int year, int? month = null, 
            bool useFinancialYear = false, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                string csvContent = "";
                string fileName = "";

                switch (reportType)
                {
                    case ReportType.Monthly:
                        if (month.HasValue)
                        {
                            var monthlyReport = await _reportingService.GetMonthlyReportAsync(user.Id, year, month.Value);
                            csvContent = GenerateMonthlyCsv(monthlyReport);
                            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Value);
                            fileName = $"Monthly_Report_{monthName}_{year}.csv";
                        }
                        break;

                    case ReportType.Annual:
                        var annualReport = await _reportingService.GetAnnualReportAsync(user.Id, year, useFinancialYear);
                        csvContent = GenerateAnnualCsv(annualReport);
                        fileName = useFinancialYear 
                            ? $"Financial_Year_Report_FY{year}_{year + 1}.csv"
                            : $"Annual_Report_{year}.csv";
                        break;

                    case ReportType.Custom:
                        if (fromDate.HasValue && toDate.HasValue)
                        {
                            var customReport = await _reportingService.GetCustomReportAsync(user.Id, fromDate.Value, toDate.Value);
                            csvContent = GenerateCustomCsv(customReport);
                            fileName = $"Custom_Report_{fromDate.Value:yyyyMMdd}_{toDate.Value:yyyyMMdd}.csv";
                        }
                        break;
                }

                if (string.IsNullOrEmpty(csvContent))
                {
                    return NotFound();
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading report for user {UserId}", User.Identity?.Name);
                return StatusCode(500, "Error generating report download.");
            }
        }

        private List<int> GetAvailableYears()
        {
            var currentYear = DateTime.Now.Year;
            var years = new List<int>();
            
            // Provide years from 2020 to current year + 1
            for (int year = 2020; year <= currentYear + 1; year++)
            {
                years.Add(year);
            }
            
            return years;
        }

        private string GenerateMonthlyCsv(MonthlyReportData report)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Monthly Report - {report.MonthName} {report.Year}");
            csv.AppendLine();
            csv.AppendLine($"Total Amount,{report.TotalAmount:C}");
            csv.AppendLine($"Tax Deductible Amount,{report.TaxDeductibleAmount:C}");
            csv.AppendLine($"Recurring Amount,{report.RecurringAmount:C}");
            csv.AppendLine($"Total Transactions,{report.TotalTransactions}");
            csv.AppendLine();
            csv.AppendLine("Category Breakdown:");
            csv.AppendLine("Category,Amount");
            
            foreach (var category in report.CategoryBreakdown)
            {
                csv.AppendLine($"{category.Key},{category.Value:C}");
            }
            
            csv.AppendLine();
            csv.AppendLine("Currency Breakdown:");
            csv.AppendLine("Currency,Amount");
            
            foreach (var currency in report.CurrencyBreakdown)
            {
                csv.AppendLine($"{currency.Key},{currency.Value}");
            }
            
            return csv.ToString();
        }

        private string GenerateAnnualCsv(AnnualReportData report)
        {
            var csv = new System.Text.StringBuilder();
            var reportTitle = report.IsFinancialYear 
                ? $"Financial Year Report - FY{report.Year}/{report.Year + 1}"
                : $"Annual Report - {report.Year}";
                
            csv.AppendLine(reportTitle);
            csv.AppendLine();
            csv.AppendLine($"Total Amount,{report.TotalAmount:C}");
            csv.AppendLine($"Tax Deductible Amount,{report.TaxDeductibleAmount:C}");
            csv.AppendLine($"Recurring Amount,{report.RecurringAmount:C}");
            csv.AppendLine($"Total Transactions,{report.TotalTransactions}");
            csv.AppendLine();
            csv.AppendLine("Monthly Breakdown:");
            csv.AppendLine("Month,Amount");
            
            foreach (var month in report.MonthlyBreakdown.OrderBy(m => m.Key))
            {
                csv.AppendLine($"{month.Key},{month.Value:C}");
            }
            
            csv.AppendLine();
            csv.AppendLine("Category Breakdown:");
            csv.AppendLine("Category,Amount");
            
            foreach (var category in report.CategoryBreakdown)
            {
                csv.AppendLine($"{category.Key},{category.Value:C}");
            }
            
            return csv.ToString();
        }

        private string GenerateCustomCsv(CustomReportData report)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Custom Report - {report.FromDate:dd MMM yyyy} to {report.ToDate:dd MMM yyyy}");
            csv.AppendLine();
            csv.AppendLine($"Total Amount,{report.TotalAmount:C}");
            csv.AppendLine($"Tax Deductible Amount,{report.TaxDeductibleAmount:C}");
            csv.AppendLine($"Recurring Amount,{report.RecurringAmount:C}");
            csv.AppendLine($"Total Transactions,{report.TotalTransactions}");
            csv.AppendLine($"Average Transaction Amount,{report.AverageTransactionAmount:C}");
            csv.AppendLine();
            csv.AppendLine("Daily Breakdown:");
            csv.AppendLine("Date,Amount");
            
            foreach (var day in report.DailyBreakdown.OrderBy(d => d.Key))
            {
                csv.AppendLine($"{day.Key},{day.Value:C}");
            }
            
            csv.AppendLine();
            csv.AppendLine("Category Breakdown:");
            csv.AppendLine("Category,Amount");
            
            foreach (var category in report.CategoryBreakdown)
            {
                csv.AppendLine($"{category.Key},{category.Value:C}");
            }
            
            return csv.ToString();
        }
    }
}