using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    Task<GetPaymentResponse?> GetPastPaymentById(Guid id);
}