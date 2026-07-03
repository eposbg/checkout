using PaymentGateway.Application.Helpers;

namespace PaymentGateway.Api.Tests.Application.Helpers
{
    public class CardHelperTests
    {
        [Theory]
        [InlineData("1111222233334444", 4, "4444")]
        [InlineData("1111222233334444", 20, "0000")]
        public void ReturnValidCard(string cardNumber, int numberOfDigits, string expectedValue) {
            // Act 
            var result = CardHelper.LastDigitsOfCardNumber(cardNumber, numberOfDigits);

            // Assert 
            Assert.Equal(expectedValue, result);
        }
    }
}
