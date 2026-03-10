using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IBankSimulatorClient
{
    Task<BankSimulatorResponse> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken);
}