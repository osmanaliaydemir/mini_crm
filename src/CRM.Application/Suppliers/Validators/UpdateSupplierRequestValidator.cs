using FluentValidation;
using Microsoft.Extensions.Localization;
using CRM.Application.Common;

namespace CRM.Application.Suppliers.Validators;

public class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateSupplierRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(_localizer["Validation_Supplier_Id_Required"]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localizer["Validation_Supplier_Name_Required"])
            .MaximumLength(200).WithMessage(_localizer["Validation_Supplier_Name_MaxLength", 200]);

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage(_localizer["Validation_Supplier_Country_MaxLength", 100])
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage(_localizer["Validation_Supplier_TaxNumber_MaxLength", 50])
            .When(x => !string.IsNullOrEmpty(x.TaxNumber));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage(_localizer["Validation_Supplier_ContactEmail_Invalid"])
            .MaximumLength(200).WithMessage(_localizer["Validation_Supplier_ContactEmail_MaxLength", 200])
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage(_localizer["Validation_Supplier_ContactPhone_MaxLength", 50])
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        RuleFor(x => x.AddressLine)
            .MaximumLength(300).WithMessage(_localizer["Validation_Supplier_AddressLine_MaxLength", 300])
            .When(x => !string.IsNullOrEmpty(x.AddressLine));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(_localizer["Validation_Supplier_Notes_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

