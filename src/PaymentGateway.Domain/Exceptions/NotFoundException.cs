namespace PaymentGateway.Domain.Exceptions
{
    public class NotFoundException(string exeption) : Exception(exeption)
    {
    }
}
