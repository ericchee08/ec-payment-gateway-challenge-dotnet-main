using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PaymentGateway.Api.HealthChecks;

public class BankSimulatorHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BankSimulatorHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BankSimulator");
            var response = await client.GetAsync("/", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Bank simulator is reachable.")
                : HealthCheckResult.Degraded($"Bank simulator returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Bank simulator is not reachable.",
                exception: ex);
        }
    }
}
