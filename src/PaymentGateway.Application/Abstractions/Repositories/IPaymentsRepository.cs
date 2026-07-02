using PaymentGateway.Domain.Entries;

namespace PaymentGateway.Application.Abstractions.Repositories
{
    public interface IPaymentsRepository
    {
        void Add(Payment payment);
        Payment? Get(Guid id);


    }
}