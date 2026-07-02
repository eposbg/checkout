namespace PaymentGateway.Application.Models.AcquiringBank
{
    public record BankPaymentResponse
    {
        public bool Authorized { get; set; }
        public string AuthorizationCode { get; set; } = string.Empty;
    }
}
