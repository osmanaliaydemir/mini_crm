using FluentValidation;
using Microsoft.Extensions.Localization;
using CRM.Application.Common;

namespace CRM.Application.Products.Validators;

public class CreateLumberVariantRequestValidator : AbstractValidator<CreateLumberVariantRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateLumberVariantRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localizer["Validation_Product_Name_Required"])
            .MaximumLength(200).WithMessage(_localizer["Validation_Product_Name_MaxLength", 200]);

        RuleFor(x => x.Species)
            .MaximumLength(100).WithMessage(_localizer["Validation_Product_Species_MaxLength", 100])
            .When(x => !string.IsNullOrEmpty(x.Species));

        RuleFor(x => x.Grade)
            .MaximumLength(50).WithMessage(_localizer["Validation_Product_Grade_MaxLength", 50])
            .When(x => !string.IsNullOrEmpty(x.Grade));

        RuleFor(x => x.UnitOfMeasure)
            .NotEmpty().WithMessage(_localizer["Validation_Product_UnitOfMeasure_Required"])
            .MaximumLength(20).WithMessage(_localizer["Validation_Product_UnitOfMeasure_MaxLength", 20]);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(_localizer["Validation_Product_Notes_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

