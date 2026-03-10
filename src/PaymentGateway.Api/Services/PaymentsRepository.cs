using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository : IPaymentsRepository
{
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

    // public void Add(PostPaymentResponse payment)
    // {
    //     Payments.Add(payment);
    // }

    public async Task<GetPaymentResponse?> GetPastPaymentById(Guid id)
    {
        var payment = _payments.FirstOrDefault(payment => payment.Id == id);

        return payment == null ? null : await GetPaymentResponse(payment);
    }

    private static async Task<GetPaymentResponse> GetPaymentResponse(PostPaymentResponse payment)
    {
        return await Task.FromResult(new GetPaymentResponse
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