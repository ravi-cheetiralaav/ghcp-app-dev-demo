namespace ExpenseTracker.Models
{
    public enum RecurringFrequency
    {
        Weekly = 1,
        Fortnightly = 2,
        Monthly = 3,
        Quarterly = 4,
        Annually = 5
    }

    public enum ExpenseCurrency
    {
        USD,
        EUR,
        GBP,
        AUD,
        CAD,
        INR,
        JPY,
        CNY
    }

    public static class CurrencyHelper
    {
        public static readonly Dictionary<ExpenseCurrency, string> CurrencySymbols = new()
        {
            { ExpenseCurrency.USD, "$" },
            { ExpenseCurrency.EUR, "€" },
            { ExpenseCurrency.GBP, "£" },
            { ExpenseCurrency.AUD, "A$" },
            { ExpenseCurrency.CAD, "C$" },
            { ExpenseCurrency.INR, "₹" },
            { ExpenseCurrency.JPY, "¥" },
            { ExpenseCurrency.CNY, "¥" }
        };

        public static string GetSymbol(string currency)
        {
            if (Enum.TryParse<ExpenseCurrency>(currency, out var currencyEnum))
            {
                return CurrencySymbols[currencyEnum];
            }
            return currency;
        }
    }
}