using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken);
    GetPaymentResponse? GetPastPaymentById(Guid id);
}