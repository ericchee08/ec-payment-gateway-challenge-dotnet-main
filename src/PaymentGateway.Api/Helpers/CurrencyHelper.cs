namespace PaymentGateway.Api.Helpers;

public static class CurrencyHelper
{
    private static readonly Dictionary<string, int> SupportedCurrenciesCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 100,
        ["GBP"] = 100,
        ["CNY"] = 10
    };

    public static bool IsCurrencySupported(string currency)
    {
        return SupportedCurrenciesCodes.ContainsKey(currency);
    }

    public static decimal MinorToMajorUnits(string currency, decimal minorAmount)
    {
        return minorAmount / SupportedCurrenciesCodes[currency];
    }
}