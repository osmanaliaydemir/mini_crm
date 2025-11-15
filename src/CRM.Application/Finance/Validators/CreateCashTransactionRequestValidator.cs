using FluentValidation;
using Microsoft.Extensions.Localization;
using CRM.Application.Common;

namespace CRM.Application.Finance.Validators;

public class CreateCashTransactionRequestValidator : AbstractValidator<CreateCashTransactionRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateCashTransactionRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage(_localizer["Validation_CashTransaction_TransactionDate_Required"]);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(_localizer["Validation_CashTransaction_Amount_GreaterThanZero"]);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage(_localizer["Validation_CashTransaction_Currency_Required"])
            .MaximumLength(10).WithMessage(_localizer["Validation_CashTransaction_Currency_MaxLength", 10]);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(_localizer["Validation_CashTransaction_Description_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage(_localizer["Validation_CashTransaction_Category_MaxLength", 100])
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}

