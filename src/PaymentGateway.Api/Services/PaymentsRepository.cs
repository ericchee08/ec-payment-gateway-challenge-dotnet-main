using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly IBankSimulatorClient _bankSimulatorClient;

    public PaymentsRepository(IBankSimulatorClient bankSimulatorClient)
    {
        _bankSimulatorClient = bankSimulatorClient;
    }

    private readonly List<PostPaymentResponse> _payments = [
        new PostPaymentResponse
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = 1234,
            ExpiryMonth = 12,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 100
        }
    ]; 

    public async Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken)
    {
        var bankResponse = await _bankSimulatorClient.ProcessPaymentAsync(request, cancellationToken);
        
        var response = BuildPaymentResponse(request, authorized: bankResponse.Authorized);
        _payments.Add(response); 

        return response;
    }

    public async Task<GetPaymentResponse?> GetPastPaymentById(Guid id)
    {
        var payment = _payments.FirstOrDefault(payment => payment.Id == id);

        return payment == null ? null : await GetPaymentResponse(payment);
    }

    private static PostPaymentResponse BuildPaymentResponse(PostPaymentRequest request, bool authorized)
    {
        var lastFourNumbersOfCardNumber = int.Parse(request.CardNumber.ToString()[^4..]);
        
        return new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = lastFourNumbersOfCardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };
    }

    private static Task<GetPaymentResponse> GetPaymentResponse(PostPaymentResponse payment)
    {
        return Task.FromResult(new GetPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        });
    }
}