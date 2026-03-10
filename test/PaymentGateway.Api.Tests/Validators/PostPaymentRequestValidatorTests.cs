using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests.Validators;

public class PostPaymentRequestValidatorTests
{
    private readonly PostPaymentRequestValidator _validator = new();

    private static PostPaymentRequest ValidRequest()
    {
        var now = DateTime.UtcNow;
        var expiryMonth = now.Month;
        var expiryYear = now.Year;
        if (expiryMonth == 12) { 
            expiryMonth = 1; expiryYear++; 
        }
        else { 
            expiryMonth++; 
        }
        return new PostPaymentRequest
        {
            CardNumber = 4111111111111111L,
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            Currency = "GBP",
            Amount = 1000,
            Cvv = 123
        };
    }

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = ValidRequest();
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CardNumber_WhenZero_ReturnsError()
    {
        var request = ValidRequest();
        request.CardNumber = 0;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.CardNumber));
    }

    [Fact]
    public void CardNumber_WhenEndsInZero_ReturnsError()
    {
        var request = ValidRequest();
        request.CardNumber = 4111111111111110L;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(PostPaymentRequest.CardNumber) &&
            e.ErrorMessage.Contains("zero", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(41111111111111L)]    // 14 numbers
    [InlineData(4111111111111111L)]  // 16 numbers
    [InlineData(2222222222222222222L)]  // 19 numers
    public void CardNumber_WhenValid_Passes(long cardNumber)
    {
        var request = ValidRequest();
        request.CardNumber = cardNumber;
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Amount_WhenZero_ReturnsError()
    {
        var request = ValidRequest();
        request.Amount = 0;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.Amount));
    }

    [Fact]
    public void Amount_WhenNegative_ReturnsError()
    {
        var request = ValidRequest();
        request.Amount = -100;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.Amount));
    }

    [Fact]
    public void Amount_WhenPositive_Passes()
    {
        var request = ValidRequest();
        request.Amount = 1;
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("GBP")]
    [InlineData("CNY")]
    public void Currency_WhenSupported_Passes(string currency)
    {
        var request = ValidRequest();
        request.Currency = currency;
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("JPY")]
    [InlineData("XYZ")]
    [InlineData("")]
    public void Currency_WhenNotSupported_ReturnsError(string currency)
    {
        var request = ValidRequest();
        request.Currency = currency;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.Currency));
    }

    [Fact]
    public void ExpiryMonth_WhenZero_ReturnsError()
    {
        var request = ValidRequest();
        request.ExpiryMonth = 0;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.ExpiryMonth));
    }

    [Fact]
    public void ExpiryMonth_WhenNegative_ReturnsError()
    {
        var request = ValidRequest();
        request.ExpiryMonth = -1;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.ExpiryMonth));
    }

    [Fact]
    public void ExpiryMonth_When13OrMore_ReturnsError()
    {
        var request = ValidRequest();
        request.ExpiryMonth = 13;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.ExpiryMonth));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void ExpiryMonth_WhenBetween1And12_Passes(int month)
    {
        var request = ValidRequest();
        request.ExpiryMonth = month;
        request.ExpiryYear = DateTime.UtcNow.Year + 1;
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ExpiryYear_WhenExpired_ReturnsError()
    {
        var request = ValidRequest();
        request.ExpiryMonth = 1;
        request.ExpiryYear = 2020;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("expired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExpiryYear_WhenCurrentOrFuture_Passes()
    {
        var request = ValidRequest();
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Cvv_WhenZero_ReturnsError()
    {
        var request = ValidRequest();
        request.Cvv = 0;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.Cvv));
    }

    [Fact]
    public void Cvv_WhenLessThan100_ReturnsError()
    {
        var request = ValidRequest();
        request.Cvv = 99;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.Cvv));
    }

    [Fact]
    public void Cvv_When10000OrMore_ReturnsError()
    {
        var request = ValidRequest();
        request.Cvv = 10000;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PostPaymentRequest.Cvv));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(999)]
    [InlineData(1000)]
    [InlineData(9999)]
    public void Cvv_When3Or4Digits_Passes(int cvv)
    {
        var request = ValidRequest();
        request.Cvv = cvv;
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}