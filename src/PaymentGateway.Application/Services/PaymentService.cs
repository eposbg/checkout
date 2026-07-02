using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.Repositories;
using PaymentGateway.Application.Abstractions.Services;

namespace PaymentGateway.Application.Services
{
    public class PaymentService(IPaymentsRepository paymentsRepository) : IPaymentService
    {
        private readonly IPaymentsRepository _paymentsRepository = paymentsRepository;

        public async Task<PostPaymentRequest> Get(Guid id)
        {
            var payment = _paymentsRepository.Get(id);
            return new PostPaymentRequest
            {
            };
        }

        public Task<PostPaymentResponse> Process(PostPaymentRequest request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
