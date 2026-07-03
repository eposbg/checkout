namespace PaymentGateway.Api.Configuration
{
    public class PaymentSettings
    {
        public string[] SupportedCurrencies { get; set; } = Array.Empty<string>();
    }
}
