using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using Moq;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private static PostPaymentRequest CreateValidPaymentRequest(
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

    private static PaymentsController CreatePaymentsController(Mock<IPaymentsRepository> mockRepository)
    {
        var logger = new Mock<ILogger<PaymentsController>>().Object;
        var controller = new PaymentsController(mockRepository.Object, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsOkWithResponse_WhenRepositoryReturnsResponse()
    {
        // Arrange
        var request = CreateValidPaymentRequest(currency: "GBP", amountMinor: 1050, cardNumber: 5500000000001005L);
        var expectedId = Guid.NewGuid();
        var expectedResponse = new PostPaymentResponse
        {
            Id = expectedId,
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = 1005,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = "GBP",
            Amount = 10.50m
        };
        var mockIPaymentsRepository = new Mock<IPaymentsRepository>();
        mockIPaymentsRepository
            .Setup(r => r.ProcessPaymentAsync(It.IsAny<PostPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        var controller = CreatePaymentsController(mockIPaymentsRepository);

        // Act
        var result = await controller.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        var response = Assert.IsType<PostPaymentResponse>(result.Value);
        Assert.Equal(expectedId, response.Id);
        Assert.Equal(10.50m, response.Amount);
        Assert.Equal(1005, response.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, response.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, response.ExpiryYear);
        Assert.Equal("GBP", response.Currency);
        Assert.Equal(PaymentStatus.Authorized, response.Status);
        mockIPaymentsRepository.Verify(
            r => r.ProcessPaymentAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void GetPastPaymentById_ReturnsOkWithPayment_WhenPaymentExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = CreateValidPaymentRequest(currency: "GBP", amountMinor: 9999, cardNumber: 5500000000001007L);
        var expectedPayment = new GetPaymentResponse
        {
            Id = id,
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = 1007,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = "GBP",
            Amount = 99.99m
        };
        var mockIPaymentsRepository = new Mock<IPaymentsRepository>();
        mockIPaymentsRepository
            .Setup(r => r.GetPastPaymentById(id))
            .Returns(expectedPayment);
        var controller = CreatePaymentsController(mockIPaymentsRepository);

        // Act
        var result = controller.GetPastPaymentById(id);

        // Assert
        var payment = Assert.IsType<GetPaymentResponse>(result.Value);
        Assert.Equal(id, payment.Id);
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal(1007, payment.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal("GBP", payment.Currency);
        Assert.Equal(99.99m, payment.Amount);
        mockIPaymentsRepository.Verify(r => r.GetPastPaymentById(id), Times.Once);
    }

    [Fact]
    public void GetPastPaymentById_ReturnsDeclinedPayment_WhenRepositoryReturnsDeclinedPayment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = CreateValidPaymentRequest(currency: "GBP", amountMinor: 9999, cardNumber: 5500000000001006L);
        var expectedPayment = new GetPaymentResponse
        {
            Id = id,
            Status = PaymentStatus.Declined,
            CardNumberLastFour = 1006,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = "GBP",
            Amount = 99.99m
        };
        var mockIPaymentsRepository = new Mock<IPaymentsRepository>();
        mockIPaymentsRepository
            .Setup(r => r.GetPastPaymentById(id))
            .Returns(expectedPayment);
        var controller = CreatePaymentsController(mockIPaymentsRepository);

        // Act
        var result = controller.GetPastPaymentById(id);

        // Assert
        var payment = Assert.IsType<GetPaymentResponse>(result.Value);
        Assert.Equal(id, payment.Id);
        Assert.Equal(PaymentStatus.Declined, payment.Status);
        Assert.Equal(1006, payment.CardNumberLastFour);
        mockIPaymentsRepository.Verify(r => r.GetPastPaymentById(id), Times.Once);
    }

    [Fact]
    public void GetPastPaymentById_Returns404_WhenPaymentNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        var mockRepository = new Mock<IPaymentsRepository>();
        mockRepository
            .Setup(r => r.GetPastPaymentById(nonexistentId))
            .Returns((GetPaymentResponse?)null);
        var controller = CreatePaymentsController(mockRepository);

        // Act
        var result = controller.GetPastPaymentById(nonexistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        mockRepository.Verify(r => r.GetPastPaymentById(nonexistentId), Times.Once);
    }
}