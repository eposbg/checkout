using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Application.Abstractions.ExternalServices;
using PaymentGateway.Application.Models.AcquiringBank;

namespace PaymentGateway.Api.Infrastructure.ExternalServices
{
    public class AcquiringBankClient(IHttpClientFactory httpClientFactory, ILogger<AcquiringBankClient> logger) : IAcquiringBankClient
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<AcquiringBankClient> _logger = logger;

        public async Task<BankPaymentResponse> ProcessPayment(PostPaymentRequest request, CancellationToken ct)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("BankSimulator");
                //_logger.LogInformation($"BackSimulator server url {client.BaseAddress}"); 
                var postRequest = new ProcessRequest
                {
                    CardNumber = request.CardNumber.ToString(),
                    ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
                    Currency = request.Currency,
                    Amount = request.Amount,
                    Cvv = request.Cvv.ToString()
                };

                var cardnumber= request.CardNumber.ToString();
                var cardNumberLast4 = cardnumber.Length <= 4  ? cardnumber : cardnumber.Substring(cardnumber.Length - 4);
                _logger.LogInformation($"Try to process payment for card ending {cardNumberLast4} for the amount of {postRequest.Amount}{postRequest.Currency}");
                var response = await client.PostAsJsonAsync("payments", postRequest, ct);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(ct);

                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex, ex.Message);
                throw;
            }

        }
    }
}
