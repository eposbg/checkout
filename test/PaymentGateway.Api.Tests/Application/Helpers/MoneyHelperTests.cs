using PaymentGateway.Application.Helpers;

namespace PaymentGateway.Api.Tests.Application.Helpers
{
    public class MoneyHelperTests
    {

        [Theory]
        [InlineData(0.01, 1)]
        [InlineData(10.50, 1050)]
        public void StripDecimalValue(decimal value, int exprected) {
            
            // Act 
            var result = MoneyHelper.ToMinorUnits(value);
            // Assert 
            Assert.Equal(exprected, result);
        }
    }
}
