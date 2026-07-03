namespace PaymentGateway.Application.Helpers
{
    public static class CardHelper
    {
        /// <summary>
        /// This method is going to return the last 4 digits of a card number. 
        /// If the card number is shorter than the number of digits expected the result would be 0000
        /// Examples: 
        ///     1111222233334444 -> 4444 if the numberOfDigits is 4
        ///     1111222233334444 -> 0000 if the numberOfDigits is 20
        /// 
        /// </summary>
        /// <param name="longCardNumber">Long card numbeer</param>
        /// <param name="numberOfDigits">Number of digits</param>
        /// <returns>String with the last card number digits</returns>
        public static string LastDigitsOfCardNumber(string longCardNumber, int numberOfDigits = 4) {
            if (!longCardNumber.Any(char.IsDigit)) {
                throw new ArgumentException("The card number should be just digits");
            }

            if (longCardNumber.Length > numberOfDigits) {
                return longCardNumber.Substring(longCardNumber.Length - numberOfDigits);
            }

            return "0000";
        }
    }
}
