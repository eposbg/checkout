using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService, IValidator<PostPaymentRequest> validator) : Controller
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IValidator<PostPaymentRequest> _validator = validator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = await _paymentService.Get(id);

        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessAsync([FromBody] PostPaymentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(
           request,
           HttpContext.RequestAborted);

        if (!validationResult.IsValid) {
            return BadRequest(validationResult.Errors);
        }

        var result = await _paymentService.Process(request, HttpContext.RequestAborted);

        return new OkObjectResult(result);
    }
}