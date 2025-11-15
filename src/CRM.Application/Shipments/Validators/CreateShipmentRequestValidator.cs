using FluentValidation;
using Microsoft.Extensions.Localization;
using CRM.Application.Common;

namespace CRM.Application.Shipments.Validators;

public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateShipmentRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage(_localizer["Validation_Shipment_SupplierId_Required"]);

        RuleFor(x => x.ReferenceNumber)
            .NotEmpty().WithMessage(_localizer["Validation_Shipment_ReferenceNumber_Required"])
            .MaximumLength(100).WithMessage(_localizer["Validation_Shipment_ReferenceNumber_MaxLength", 100]);

        RuleFor(x => x.ShipmentDate)
            .NotEmpty().WithMessage(_localizer["Validation_Shipment_ShipmentDate_Required"]);

        RuleFor(x => x.LoadingPort)
            .MaximumLength(200).WithMessage(_localizer["Validation_Shipment_LoadingPort_MaxLength", 200])
            .When(x => !string.IsNullOrEmpty(x.LoadingPort));

        RuleFor(x => x.DischargePort)
            .MaximumLength(200).WithMessage(_localizer["Validation_Shipment_DischargePort_MaxLength", 200])
            .When(x => !string.IsNullOrEmpty(x.DischargePort));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage(_localizer["Validation_Shipment_Notes_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.StageNotes)
            .MaximumLength(500).WithMessage(_localizer["Validation_Shipment_StageNotes_MaxLength", 500])
            .When(x => !string.IsNullOrEmpty(x.StageNotes));
    }
}

