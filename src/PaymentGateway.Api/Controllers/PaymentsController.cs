using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Infrastructure.Persistance;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService) : Controller
{
    private readonly IPaymentService _paymentService = paymentService;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = await _paymentService.Get(id);

        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessAsync([FromBody] PostPaymentRequest request)
    {
        var payment = await _paymentService.Process(request, HttpContext.RequestAborted);

        return new OkObjectResult(payment);
    }
}