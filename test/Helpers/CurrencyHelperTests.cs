namespace PaymentGateway.Api.Tests.Helpers;

public class CurrencyHelperTests
{
    [Theory]
    [InlineData("GBP", 1050, 10.50)]
    [InlineData("GBP", 1, 0.01)]
    public void MinorToMajorUnits_GBP_PennyScale(string currency, int minor, decimal expectedMajor)
    {
        Assert.Equal(expectedMajor, CurrencyHelper.MinorToMajorUnits(currency, minor));
    }
    [Theory]
    [InlineData("USD", 1050, 10.50)]
    [InlineData("USD", 1, 0.01)]
    public void MinorToMajorUnits_USD_CentScale(string currency, int minor, decimal expectedMajor)
    {
        Assert.Equal(expectedMajor, CurrencyHelper.MinorToMajorUnits(currency, minor));
    }

    [Theory]
    [InlineData("CNY", 1050, 105.0)]
    [InlineData("CNY", 10, 1.0)]
    [InlineData("CNY", 1, 0.1)]
    public void MinorToMajorUnits_CNY_JiaoScale(string currency, int minor, decimal expectedMajor)
    {
        Assert.Equal(expectedMajor, CurrencyHelper.MinorToMajorUnits(currency, minor));
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("GBP")]
    [InlineData("CNY")]
    [InlineData("usd")]
    [InlineData("gbp")]
    public void IsSupported_WhenAllowedCurrency_ReturnsTrue(string currency)
    {
        Assert.True(CurrencyHelper.IsCurrencySupported(currency));
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("JPY")]
    [InlineData("")]
    public void IsSupported_WhenNotAllowed_ReturnsFalse(string currency)
    {
        Assert.False(CurrencyHelper.IsCurrencySupported(currency));
    }
}