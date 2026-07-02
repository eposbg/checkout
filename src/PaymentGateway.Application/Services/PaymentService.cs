using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Abstractions.Repositories;
using PaymentGateway.Application.Abstractions.Services;
using PaymentGateway.Domain.Entries;

namespace PaymentGateway.Application.Services
{
    public class PaymentService(IPaymentsRepository paymentsRepository, IAcquiringBankClient acquiringBankClient, ILogger<PaymentService> logger) : IPaymentService
    {
        private readonly IPaymentsRepository _paymentsRepository = paymentsRepository;

        public async Task<PostPaymentRequest> Get(Guid id)
        {
            var payment = _paymentsRepository.Get(id);
            return new PostPaymentRequest
            {
            };
        }

        public async Task<PostPaymentResponse> Process(PostPaymentRequest request, CancellationToken ct)
        {
            try
            {
                var response = await acquiringBankClient.ProcessPayment(request, ct);
                if (!response.Authorized)
                {
                    _paymentsRepository.Add(new Payment {
                         
                    });
                }

                return new PostPaymentResponse
                {
                    Status = response.Authorized ? Domain.Enums.PaymentStatus.Authorized : Domain.Enums.PaymentStatus.Declined
                };

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
            
        }
    }
}
