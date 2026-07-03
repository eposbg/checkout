using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Application.Models.AcquiringBank;

namespace PaymentGateway.Application.Abstractions.ExternalServices
{
    public interface IAcquiringBankClient
    {
        Task<BankPaymentResponse> ProcessPayment(BankPaymentRequest request, CancellationToken ct);
    }
}
