using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Application.Abstractions.Services
{
    public interface IPaymentService
    {
        Task<PostPaymentRequest?> Get(Guid id);
        Task<PostPaymentResponse?> Process(PostPaymentRequest request, CancellationToken ct);
    }
}
