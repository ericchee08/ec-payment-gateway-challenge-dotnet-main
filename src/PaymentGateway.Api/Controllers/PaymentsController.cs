using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentsRepository paymentsRepository, ILogger<PaymentsController> logger)
    {
        _paymentsRepository = paymentsRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken)
    {
        return await _paymentsRepository.ProcessPaymentAsync(request, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GetPaymentResponse> GetPastPaymentById(Guid id)
    {
        var payment = _paymentsRepository.GetPastPaymentById(id);
        if (payment == null)
        {
            _logger.LogWarning("Payment not found. PaymentId: {PaymentId}, TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return NotFound();
        }

        return payment;
    }
}