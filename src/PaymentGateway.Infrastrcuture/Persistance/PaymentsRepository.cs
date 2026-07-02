using PaymentGateway.Application.Abstractions.Repositories;
using PaymentGateway.Domain.Entries;

namespace PaymentGateway.Api.Infrastructure.Persistance;

public class PaymentsRepository : IPaymentsRepository
{
    public List<Payment> Payments = new();

    public void Add(Payment payment)
    {
        Payments.Add(payment);
    }

    public Payment? Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
   
    public async Task CreateAsync(Payment payment)
    {
        var existingPayment = Payments.FirstOrDefault(x => x.Id == payment.Id);
        if (existingPayment == null)
        {
            Payments.Add(payment);
        }

        existingPayment.Currency = payment.Currency;
        //existingPayment.Amount.


        
    }
}