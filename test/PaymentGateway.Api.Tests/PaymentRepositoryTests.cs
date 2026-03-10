using Moq;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Tests;

public class PaymentsRepositoryTests
{
    private static PostPaymentRequest CreateRequest(
        string currency = "GBP",
        int amountMinor = 1050,
        long cardNumber = 5500000000001005L)
    {
        var now = DateTime.UtcNow;
        var expiryMonth = now.Month == 12 ? 1 : now.Month + 1;
        var expiryYear = now.Month == 12 ? now.Year + 1 : now.Year;
        return new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            Currency = currency,
            Amount = amountMinor,
            Cvv = 123
        };
    }

    private static PaymentsRepository CreateRepository(Mock<IBankSimulatorClient> mockBankSimulatorClient)
    {
        return new PaymentsRepository(mockBankSimulatorClient.Object);
    }

    private static Mock<IBankSimulatorClient> CreateMockBankSimulatorClient(bool authorized = true)
    {
        var mock = new Mock<IBankSimulatorClient>();
        mock.Setup(b => b.ProcessPaymentAsync(It.IsAny<PostPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BankSimulatorResponse { Authorized = authorized });
        return mock;
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsAuthorizedPayment_WhenBankAuthorizes()
    {
        var request = CreateRequest(currency: "GBP", amountMinor: 1050, cardNumber: 5500000000001005L);
        var mockBankSimulatorClient = CreateMockBankSimulatorClient();
        var repository = CreateRepository(mockBankSimulatorClient);

        var result = await repository.ProcessPaymentAsync(request, CancellationToken.None);

        Assert.Equal(PaymentStatus.Authorized, result.Status);
        Assert.Equal(1005, result.CardNumberLastFour);
        Assert.Equal(10.50m, result.Amount);
        Assert.Equal(request.ExpiryMonth, result.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, result.ExpiryYear);
        Assert.Equal("GBP", result.Currency);
        Assert.NotEqual(Guid.Empty, result.Id);
        mockBankSimulatorClient.Verify(
            b => b.ProcessPaymentAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsDeclinedPayment_WhenBankDeclines()
    {
        var request = CreateRequest(cardNumber: 5500000000001006L);
        var mockBankSimulatorClient = CreateMockBankSimulatorClient(authorized: false);
        var repository = CreateRepository(mockBankSimulatorClient);

        var result = await repository.ProcessPaymentAsync(request, CancellationToken.None);

        Assert.Equal(PaymentStatus.Declined, result.Status);
        Assert.Equal(1006, result.CardNumberLastFour);
        Assert.Equal(10.50m, result.Amount);
        Assert.Equal(request.ExpiryMonth, result.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, result.ExpiryYear);
        Assert.Equal("GBP", result.Currency);
        mockBankSimulatorClient.Verify(
            b => b.ProcessPaymentAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("GBP", 9999, 99.99)]
    [InlineData("USD", 9999, 99.99)]
    [InlineData("CNY", 9999, 999.9)]
    public async Task ProcessPaymentAsync_ConvertsAmountToMajorUnits_UsingCurrency(string currency, int amountMinor, decimal expectedAmount)
    {
        var request = CreateRequest(currency: currency, amountMinor: amountMinor);
        var mockBankSimulatorClient = CreateMockBankSimulatorClient();
        var repository = CreateRepository(mockBankSimulatorClient);

        var result = await repository.ProcessPaymentAsync(request, CancellationToken.None);

        Assert.Equal(expectedAmount, result.Amount);
    }

    [Fact]
    public async Task GetPastPaymentById_ReturnsStoredPayment_AfterProcessPayment()
    {
        var request = CreateRequest(currency: "GBP", amountMinor: 9999, cardNumber: 5500000000001007L);
        var mockBankSimulatorClient = CreateMockBankSimulatorClient();
        var repository = CreateRepository(mockBankSimulatorClient);

        var createdPayment = await repository.ProcessPaymentAsync(request, CancellationToken.None);
        var retrievedPayment = repository.GetPastPaymentById(createdPayment.Id);

        Assert.Equal(createdPayment.Id, retrievedPayment!.Id);
        Assert.Equal(createdPayment.Status, retrievedPayment.Status);
        Assert.Equal(createdPayment.CardNumberLastFour, retrievedPayment.CardNumberLastFour);
        Assert.Equal(createdPayment.ExpiryMonth, retrievedPayment.ExpiryMonth);
        Assert.Equal(createdPayment.ExpiryYear, retrievedPayment.ExpiryYear);
        Assert.Equal(createdPayment.Currency, retrievedPayment.Currency);
        Assert.Equal(createdPayment.Amount, retrievedPayment.Amount);
    }

    [Fact]
    public async Task GetPastPaymentById_ReturnsNull_WhenIdDoesNotMatchAnyStoredPayment()
    {
        var request = CreateRequest();
        var mockBankSimulatorClient = CreateMockBankSimulatorClient();
        var repository = CreateRepository(mockBankSimulatorClient);
        await repository.ProcessPaymentAsync(request, CancellationToken.None);

        var result = repository.GetPastPaymentById(Guid.NewGuid());

        Assert.Null(result);
    }
}