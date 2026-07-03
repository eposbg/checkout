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
using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Application.Services
{
    public class PaymentService(
            IPaymentsRepository paymentsRepository, IAcquiringBankClient acquiringBankClient,
            ILogger<PaymentService> logger) : IPaymentService
    {
        private readonly IPaymentsRepository _paymentsRepository = paymentsRepository;
        private readonly ILogger<PaymentService> _logger = logger;

        public async Task<PostPaymentResponse?> Get(Guid id)
        {
            var payment = _paymentsRepository.Get(id);
            if (payment == null)
                return null;

            return new PostPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                CardNumberLastFour = payment.CardNumberLastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
            };
        }

        public async Task<PostPaymentResponse> Process(PostPaymentRequest request, CancellationToken ct)
        {

            var bankRequest = new BankPaymentRequest
            {
                CardNumber = request.CardNumber.ToString(),
                ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
                Currency = request.Currency,
                Amount = MoneyHelper.ToMinorUnits(request.Amount),
                Cvv = request.Cvv.ToString()
            };

            BankPaymentResponse response;
            try
            {
                response = await acquiringBankClient.ProcessPayment(bankRequest, ct);
            }
            catch (BankServiceUnavailableException ex)
            {
                _logger.LogCritical(ex, "Back Service is unavalable");
                throw;
            }

            var newPayment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = MoneyHelper.ToMinorUnits(request.Amount),
                CardNumberLastFour = CardHelper.LastDigitsOfCardNumber(request.CardNumber, 4),
                Status = response.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                Currency = request.Currency,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear
            };
            _paymentsRepository.Add(newPayment);

            return await Get(newPayment.Id);
        }
    }
}
