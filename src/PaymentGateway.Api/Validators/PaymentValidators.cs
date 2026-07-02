using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PostPaymentRequest>
    {

        public PaymentRequestValidator()
        {
            RuleFor(x => x.CardNumber)
                .Must(ValidCreditCardNumber)
                .WithMessage("The card number is invalid. " +
                "  - Card numbers can contain between 14-19 characters long. " +
                "  - Must only contain numeric characters ");

            RuleFor(x => x.ExpiryMonth)
                .NotEmpty()
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(12)
                .WithMessage("ExpiryMonth is required and needs to be a value between 1-12");

            RuleFor(x => x.ExpiryYear)
                .NotEmpty()
                .Must((request, expiryYear) => CheckExpirationDate(request))
                .WithMessage("ExpiryYear is required");

            RuleFor(x => x.Amount)
                   .GreaterThan(0)
                   .WithMessage("Amount must be greater than 0");

        }

        public bool ValidCreditCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return false;
            }

            if (cardNumber.Length < 14 || cardNumber.Length > 19)
                return false;

            if (!cardNumber.All(char.IsDigit))
                return false;

            return true;
        }

        public bool CheckExpirationDate(PostPaymentRequest request)
        {
            if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12 || request.ExpiryYear < DateTime.Now.Year)
                return false;

            var expiresAt = new DateTime(
                request.ExpiryYear,
                request.ExpiryMonth,
                DateTime.DaysInMonth(request.ExpiryYear, request.ExpiryMonth));


            return expiresAt.Date >= DateTime.Now.Date;
        }
    }
}
