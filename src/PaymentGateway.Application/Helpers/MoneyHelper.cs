namespace PaymentGateway.Application.Helpers
{
    public static class MoneyHelper
    {
        public static long ToMinorUnits(decimal value) {
            decimal minorUnits = value * 100;
            return (long)Math.Round(minorUnits, MidpointRounding.AwayFromZero);
        }

        public static decimal FromMinorUnits(long value)
        {
            return value / 100m;
        }
    }
}
