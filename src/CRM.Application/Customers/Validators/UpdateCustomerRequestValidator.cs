using FluentValidation;
using Microsoft.Extensions.Localization;
using CRM.Application.Common;

namespace CRM.Application.Customers.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UpdateCustomerRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(_localizer["Validation_Customer_Id_Required"]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localizer["Validation_Customer_Name_Required"])
            .MaximumLength(200).WithMessage(_localizer["Validation_Customer_Name_MaxLength", 200]);

        RuleFor(x => x.LegalName)
            .MaximumLength(200).WithMessage(_localizer["Validation_Customer_LegalName_MaxLength", 200])
            .When(x => !string.IsNullOrEmpty(x.LegalName));

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50).WithMessage(_localizer["Validation_Customer_TaxNumber_MaxLength", 50])
            .When(x => !string.IsNullOrEmpty(x.TaxNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(_localizer["Validation_Customer_Email_Invalid"])
            .MaximumLength(200).WithMessage(_localizer["Validation_Customer_Email_MaxLength", 200])
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage(_localizer["Validation_Customer_Phone_MaxLength", 50])
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage(_localizer["Validation_Customer_Address_MaxLength", 300])
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Segment)
            .MaximumLength(100).WithMessage(_localizer["Validation_Customer_Segment_MaxLength", 100])
            .When(x => !string.IsNullOrEmpty(x.Segment));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(_localizer["Validation_Customer_Notes_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.PrimaryContactName)
            .MaximumLength(150).WithMessage(_localizer["Validation_Customer_PrimaryContactName_MaxLength", 150])
            .When(x => !string.IsNullOrEmpty(x.PrimaryContactName));

        RuleFor(x => x.PrimaryContactPosition)
            .MaximumLength(100).WithMessage(_localizer["Validation_Customer_PrimaryContactPosition_MaxLength", 100])
            .When(x => !string.IsNullOrEmpty(x.PrimaryContactPosition));

        RuleFor(x => x.PrimaryContactEmail)
            .EmailAddress().WithMessage(_localizer["Validation_Customer_PrimaryContactEmail_Invalid"])
            .MaximumLength(200).WithMessage(_localizer["Validation_Customer_PrimaryContactEmail_MaxLength", 200])
            .When(x => !string.IsNullOrEmpty(x.PrimaryContactEmail));

        RuleFor(x => x.PrimaryContactPhone)
            .MaximumLength(50).WithMessage(_localizer["Validation_Customer_PrimaryContactPhone_MaxLength", 50])
            .When(x => !string.IsNullOrEmpty(x.PrimaryContactPhone));
    }
}

