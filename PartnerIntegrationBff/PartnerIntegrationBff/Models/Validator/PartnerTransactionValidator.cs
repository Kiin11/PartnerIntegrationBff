using FluentValidation;
using PartnerIntegrationBff.Constant;
using PartnerIntegrationBff.Models.Request;

namespace PartnerIntegrationBff.Models.Validator
{
    public class PartnerTransactionValidator : AbstractValidator<PartnerTransactionRequest>
    {
        public PartnerTransactionValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty().WithMessage("PartnerId is required.")
                .MaximumLength(50).WithMessage("PartnerId cannot exceed 50 characters.");
            RuleFor(x => x.TransactionReference)
                .NotEmpty().WithMessage("TransactionReference is required.")
                .MaximumLength(100).WithMessage("TransactionReference cannot exceed 100 characters.");
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Must(BeAValidCurrency).WithMessage("Currency must be a valid ISO currency code.");
            RuleFor(x => x.Timestamp)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Timestamp cannot be in the future.");
        }

        private static bool BeAValidCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                return false;
            }

            return CurrencyConstant.SupportedCurrencies.Contains(currency.Trim());
        }
    }
}
