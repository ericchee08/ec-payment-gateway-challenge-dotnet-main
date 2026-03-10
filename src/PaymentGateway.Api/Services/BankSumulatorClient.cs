using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public class BankSimulatorClient : IBankSimulatorClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankSimulatorClient> _logger;

    public BankSimulatorClient(HttpClient httpClient, ILogger<BankSimulatorClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private const string PaymentsPath = "payments";

    public async Task<BankSimulatorResponse> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var bankRequest = MapToBankRequest(request);
        var response = await _httpClient.PostAsJsonAsync(PaymentsPath, bankRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Bank simulator returned non-success. StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}, ResponseBody: {ResponseBody}",
                (int)response.StatusCode, response.ReasonPhrase, body);
        }

        var bankResponse = await response.Content.ReadFromJsonAsync<BankSimulatorResponse>(cancellationToken);
        if (bankResponse is null)
        {
            _logger.LogError("Bank simulator returned invalid response.");
            throw new InvalidOperationException("Bank simulator returned an invalid response.");
        }

        if (!bankResponse.Authorized)
        {
            _logger.LogWarning(
                "Bank simulator declined payment. AuthorizationCode: {AuthorizationCode}",
                bankResponse.AuthorizationCode);
        }

        return bankResponse ;
    }

    private static BankSimulatorRequest MapToBankRequest(PostPaymentRequest request)    
    {
        var bankRequest = new BankSimulatorRequest
        {
            CardNumber = request.CardNumber.ToString(),
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear:D4}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv.ToString()
        };

        return bankRequest;
    }
}