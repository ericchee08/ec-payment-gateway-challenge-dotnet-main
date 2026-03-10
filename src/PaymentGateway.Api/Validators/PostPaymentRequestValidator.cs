using FluentValidation;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    public PostPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .WithMessage("Card number is required.")
            .Must(value => value.ToString().Length is >= 14 and <= 19)
            .WithMessage("Card number must be between 14 and 19 characters.")
            .Must(value => long.TryParse(value.ToString(), out var result))
            .WithMessage("Card number must only contain numeric characters.")
            .Must(value => value % 10 != 0)
            .WithMessage("Card number can't end in zero.");
            
        RuleFor(x => x.ExpiryMonth)
            .NotEmpty()
            .WithMessage("Expiry month is required.")
            .GreaterThan(0)
            .WithMessage("Expiry month must be a positive integer.")
            .LessThan(13)
            .WithMessage("Expiry month must be less than 13.");

        RuleFor(x => x.ExpiryYear)
            .NotEmpty()
            .WithMessage("Expiry year is required.");

        RuleFor(x => x)
            .Must(x => IsExpiryAfterCurrentDate(x.ExpiryMonth, x.ExpiryYear))
            .WithMessage("Card has expired. Expiry date must be after the current date.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be 3 characters.")
            .Must(value => value.All(char.IsLetter))
            .WithMessage("Currency must only contain letters.")
            .Must(CurrencyHelper.IsCurrencySupported)
            .WithMessage("Currency must be one of: USD, GBP, CNY.");

        RuleFor(x => x.Amount)
            .NotEmpty()
            .WithMessage("Amount is required.")
            .GreaterThan(0)
            .WithMessage("Amount must be a positive integer.");

        RuleFor(x => x.Cvv)
            .NotEmpty()
            .WithMessage("CVV is required.")
            .GreaterThan(100)
            .WithMessage("CVV must be a minimum of 3 digits.")
            .LessThan(10000)
            .WithMessage("CVV must be a maximum of 4 digits.");
    }

    private static bool IsExpiryAfterCurrentDate(int month, int year)
    {
        if (month is < 1 or > 12 || year < 1 || year > 9999)
            return true; 
        var today = DateTime.UtcNow;
        var expiryStart = new DateTime(year, month, 1);
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);
        
        return expiryStart >= currentMonthStart;
    }
}