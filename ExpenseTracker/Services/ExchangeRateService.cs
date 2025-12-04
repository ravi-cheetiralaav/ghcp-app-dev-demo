using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using ExpenseTracker.Services;

namespace ExpenseTracker.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExchangeRateService> _logger;
        private const string API_URL = "https://v6.exchangerate-api.com/v6/d2f587c45da8035c52eabbb2/latest/AUD";
        private const string CACHE_KEY = "exchange_rates_aud";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1); // Cache for 1 hour
        private static readonly TimeSpan FALLBACK_CACHE_DURATION = TimeSpan.FromDays(1); // Keep fallback for 24 hours

        public ExchangeRateService(HttpClient httpClient, IMemoryCache cache, ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            
            // Set timeout for API calls
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<ExchangeRateData?> GetLatestRatesAsync(string baseCurrency = "AUD")
        {
            // Check cache first
            if (_cache.TryGetValue(CACHE_KEY, out ExchangeRateData? cachedData) && cachedData != null)
            {
                if (DateTime.UtcNow - cachedData.LastUpdated < CACHE_DURATION)
                {
                    _logger.LogInformation("Using cached exchange rates for {BaseCurrency}", baseCurrency);
                    return cachedData;
                }
            }

            try
            {
                _logger.LogInformation("Fetching latest exchange rates for {BaseCurrency}", baseCurrency);
                
                var response = await _httpClient.GetAsync(API_URL);
                response.EnsureSuccessStatusCode();
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ExchangeRateApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (apiResponse?.Result == "success" && apiResponse.ConversionFactors != null)
                {
                    var exchangeData = new ExchangeRateData
                    {
                        BaseCurrency = baseCurrency,
                        LastUpdated = DateTime.UtcNow,
                        ConversionRates = apiResponse.ConversionFactors,
                        IsSuccess = true
                    };

                    // Cache the successful result
                    _cache.Set(CACHE_KEY, exchangeData, CACHE_DURATION);
                    
                    // Also store as fallback with longer duration
                    _cache.Set($"{CACHE_KEY}_fallback", exchangeData, FALLBACK_CACHE_DURATION);
                    
                    _logger.LogInformation("Successfully fetched and cached exchange rates");
                    return exchangeData;
                }
                else
                {
                    _logger.LogWarning("API returned unsuccessful result: {Result}", apiResponse?.Result);
                    return await GetFallbackRates();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rates from API");
                return await GetFallbackRates();
            }
        }

        private async Task<ExchangeRateData?> GetFallbackRates()
        {
            // Try to get fallback from cache
            if (_cache.TryGetValue($"{CACHE_KEY}_fallback", out ExchangeRateData? fallbackData) && fallbackData != null)
            {
                _logger.LogInformation("Using fallback exchange rates from cache");
                fallbackData.ErrorMessage = "Using cached rates due to API unavailability";
                return fallbackData;
            }

            // If no fallback available, return error state with basic rates
            _logger.LogWarning("No fallback exchange rates available, using default rates");
            return new ExchangeRateData
            {
                BaseCurrency = "AUD",
                LastUpdated = DateTime.UtcNow,
                ConversionRates = GetDefaultRates(),
                IsSuccess = false,
                ErrorMessage = "Exchange rate service unavailable. Using approximate rates."
            };
        }

        private Dictionary<string, decimal> GetDefaultRates()
        {
            // Approximate rates as fallback (these should be updated periodically)
            return new Dictionary<string, decimal>
            {
                { "USD", 0.65m },
                { "EUR", 0.61m },
                { "GBP", 0.52m },
                { "AUD", 1.0m },
                { "CAD", 0.91m },
                { "INR", 55.0m },
                { "JPY", 96.0m },
                { "CNY", 4.7m }
            };
        }

        public async Task<decimal> ConvertToAudAsync(decimal amount, string fromCurrency)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency) || fromCurrency.ToUpper() == "AUD")
            {
                return amount; // Already in AUD or invalid currency
            }

            var rates = await GetLatestRatesAsync();
            if (rates?.ConversionRates != null && rates.ConversionRates.TryGetValue(fromCurrency.ToUpper(), out var rate))
            {
                // Convert from foreign currency to AUD
                // If base is AUD, the rate gives us how much foreign currency = 1 AUD
                // So to convert from foreign to AUD: amount / rate
                return Math.Round(amount / rate, 2);
            }

            _logger.LogWarning("Could not find exchange rate for currency {Currency}, returning original amount", fromCurrency);
            return amount; // Return original if conversion fails
        }

        public async Task<CurrencyConversionResult> ConvertWithDetailsAsync(decimal amount, string fromCurrency, string toCurrency = "AUD")
        {
            var result = new CurrencyConversionResult
            {
                OriginalAmount = amount,
                OriginalCurrency = fromCurrency.ToUpper(),
                ConvertedCurrency = toCurrency.ToUpper()
            };

            if (fromCurrency.ToUpper() == toCurrency.ToUpper())
            {
                result.ConvertedAmount = amount;
                result.ExchangeRate = 1.0m;
                result.RateTimestamp = DateTime.UtcNow;
                result.IsSuccess = true;
                return result;
            }

            try
            {
                var rates = await GetLatestRatesAsync();
                if (rates?.ConversionRates != null && rates.ConversionRates.TryGetValue(fromCurrency.ToUpper(), out var rate))
                {
                    result.ConvertedAmount = Math.Round(amount / rate, 2);
                    result.ExchangeRate = rate;
                    result.RateTimestamp = rates.LastUpdated;
                    result.IsSuccess = rates.IsSuccess;
                    result.ErrorMessage = rates.ErrorMessage;
                }
                else
                {
                    result.ConvertedAmount = amount;
                    result.ExchangeRate = 1.0m;
                    result.RateTimestamp = DateTime.UtcNow;
                    result.IsSuccess = false;
                    result.ErrorMessage = $"Exchange rate not found for {fromCurrency}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during currency conversion from {From} to {To}", fromCurrency, toCurrency);
                result.ConvertedAmount = amount;
                result.ExchangeRate = 1.0m;
                result.RateTimestamp = DateTime.UtcNow;
                result.IsSuccess = false;
                result.ErrorMessage = "Currency conversion failed";
            }

            return result;
        }

        public void ClearCache()
        {
            _cache.Remove(CACHE_KEY);
            _cache.Remove($"{CACHE_KEY}_fallback");
            _logger.LogInformation("Exchange rate cache cleared");
        }
    }

    // DTO for API response
    internal class ExchangeRateApiResponse
    {
        public string? Result { get; set; }
        public string? Documentation { get; set; }
        public string? TermsOfUse { get; set; }
        public long? TimeLastUpdateUnix { get; set; }
        public string? TimeLastUpdateUtc { get; set; }
        public long? TimeNextUpdateUnix { get; set; }
        public string? TimeNextUpdateUtc { get; set; }
        public string? BaseCode { get; set; }
        public Dictionary<string, decimal>? ConversionFactors { get; set; }
    }
}