using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators
{
    public class PaymentRequestValidator: AbstractValidator<PostPaymentRequest>
    {

        public PaymentRequestValidator()
        {
            RuleFor(x=>x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");

        }
    }
}
