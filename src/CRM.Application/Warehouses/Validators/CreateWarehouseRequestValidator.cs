using FluentValidation;
using Microsoft.Extensions.Localization;
using CRM.Application.Common;

namespace CRM.Application.Warehouses.Validators;

public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateWarehouseRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localizer["Validation_Warehouse_Name_Required"])
            .MaximumLength(200).WithMessage(_localizer["Validation_Warehouse_Name_MaxLength", 200]);

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage(_localizer["Validation_Warehouse_Location_MaxLength", 200])
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.ContactPerson)
            .MaximumLength(150).WithMessage(_localizer["Validation_Warehouse_ContactPerson_MaxLength", 150])
            .When(x => !string.IsNullOrEmpty(x.ContactPerson));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(50).WithMessage(_localizer["Validation_Warehouse_ContactPhone_MaxLength", 50])
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(_localizer["Validation_Warehouse_Notes_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

