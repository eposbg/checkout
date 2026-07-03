using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Domain.Entries
{
    public class Payment
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public string CardNumberLastFour { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
