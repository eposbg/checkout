using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Abstractions.Repositories;
using PaymentGateway.Application.Abstractions.Services;
using PaymentGateway.Application.Helpers;
using PaymentGateway.Application.Models.AcquiringBank;
using PaymentGateway.Domain.Entries;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Services
{
    public class PaymentService(IPaymentsRepository paymentsRepository, IAcquiringBankClient acquiringBankClient, ILogger<PaymentService> logger) : IPaymentService
    {
        private readonly IPaymentsRepository _paymentsRepository = paymentsRepository;

        public async Task<PostPaymentResponse> Get(Guid id)
        {
            var payment = _paymentsRepository.Get(id);
            return new PostPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status.ToString(),
                Amount = MoneyHelper.FromMinorUnits(payment.Amount),
                CardNumberLastFour = payment.CardNumberLastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
            };
        }

        public async Task<PostPaymentResponse> Process(PostPaymentRequest request, CancellationToken ct)
        {
            try
            {
                var bankRequest = new BankPaymentRequest
                {
                    CardNumber = request.CardNumber.ToString(),
                    ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
                    Currency = request.Currency,
                    Amount = MoneyHelper.ToMinorUnits(request.Amount),
                    Cvv = request.Cvv.ToString()
                };
                var response = await acquiringBankClient.ProcessPayment(bankRequest, ct);

                var newPayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    Amount = MoneyHelper.ToMinorUnits(request.Amount),
                    CardNumberLastFour = CardHelper.LastDigitsOfCardNumber(request.CardNumber, 4),
                    Status = response.Authorized  ? PaymentStatus.Authorized : PaymentStatus.Declined,
                    Currency = request.Currency,
                    ExpiryMonth = request.ExpiryMonth,
                    ExpiryYear = request.ExpiryYear
                };
                _paymentsRepository.Add(newPayment);
                
               return await Get(newPayment.Id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, ex.Message);
                throw;
            }

        }
    }
}
