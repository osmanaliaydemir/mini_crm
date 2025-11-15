using FluentValidation;
using Microsoft.Extensions.Localization;
using CRM.Application.Common;

namespace CRM.Application.Finance.Validators;

public class CreatePaymentPlanRequestValidator : AbstractValidator<CreatePaymentPlanRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreatePaymentPlanRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage(_localizer["Validation_PaymentPlan_CustomerId_Required"]);

        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage(_localizer["Validation_PaymentPlan_ShipmentId_Required"]);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage(_localizer["Validation_PaymentPlan_TotalAmount_GreaterThanZero"]);

        RuleFor(x => x.Currency)
            .MaximumLength(10).WithMessage(_localizer["Validation_PaymentPlan_Currency_MaxLength", 10])
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage(_localizer["Validation_PaymentPlan_StartDate_Required"]);

        RuleFor(x => x.PeriodicityWeeks)
            .GreaterThan(0).WithMessage(_localizer["Validation_PaymentPlan_PeriodicityWeeks_GreaterThanZero"]);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(_localizer["Validation_PaymentPlan_Notes_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleForEach(x => x.Installments)
            .SetValidator(new CreatePaymentInstallmentRequestValidator(_localizer))
            .When(x => x.Installments != null && x.Installments.Any());
    }
}

public class CreatePaymentInstallmentRequestValidator : AbstractValidator<CreatePaymentInstallmentRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreatePaymentInstallmentRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.InstallmentNumber)
            .GreaterThan(0).WithMessage(_localizer["Validation_PaymentInstallment_InstallmentNumber_GreaterThanZero"]);

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage(_localizer["Validation_PaymentInstallment_DueDate_Required"]);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage(_localizer["Validation_PaymentInstallment_Amount_GreaterThanZero"]);
    }
}

