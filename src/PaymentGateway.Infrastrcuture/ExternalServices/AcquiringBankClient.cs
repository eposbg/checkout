using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Models.AcquiringBank;
using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Api.Infrastructure.ExternalServices
{
    public class AcquiringBankClient(IHttpClientFactory httpClientFactory, ILogger<AcquiringBankClient> logger) : IAcquiringBankClient
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<AcquiringBankClient> _logger = logger;

        public async Task<BankPaymentResponse> ProcessPayment(BankPaymentRequest request, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient("BankSimulator");

            var cardnumber = request.CardNumber.ToString();
            var cardNumberLast4 = cardnumber.Length <= 4 ? cardnumber : cardnumber.Substring(cardnumber.Length - 4);
            _logger.LogInformation($"Try to process payment for card ending {cardNumberLast4} for the amount of {request.Amount}{request.Currency}");
            var response = await client.PostAsJsonAsync("payments", request, ct);
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                throw new BankServiceUnavailableException();
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(ct);
            return result;
        }
    }
}
