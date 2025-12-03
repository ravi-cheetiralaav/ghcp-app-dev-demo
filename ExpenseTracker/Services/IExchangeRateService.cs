using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateData?> GetLatestRatesAsync(string baseCurrency = "AUD");
        Task<decimal> ConvertToAudAsync(decimal amount, string fromCurrency);
        Task<CurrencyConversionResult> ConvertWithDetailsAsync(decimal amount, string fromCurrency, string toCurrency = "AUD");
        void ClearCache();
    }

    public class ExchangeRateData
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, decimal> ConversionRates { get; set; } = new();
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CurrencyConversionResult
    {
        public decimal OriginalAmount { get; set; }
        public string OriginalCurrency { get; set; } = string.Empty;
        public decimal ConvertedAmount { get; set; }
        public string ConvertedCurrency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public DateTime RateTimestamp { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}