using System.Linq;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Shipments;

public class DetailsModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public DetailsModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ShipmentDetailView? Shipment { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Supplier)
            .Include(s => s.Customer)
            .Include(s => s.Stages)
            .Include(s => s.CustomsProcess)
            .Include(s => s.Items)
                .ThenInclude(i => i.Variant)
            .Include(s => s.TransportUnits)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment is null)
        {
            return NotFound();
        }

        var stages = shipment.Stages
            .OrderBy(stage => stage.StartedAt)
            .Select(stage => new ShipmentStageView(
                stage.Status,
                stage.StartedAt,
                stage.CompletedAt,
                stage.Notes))
            .ToList();

        var items = shipment.Items
            .Select(item => new ShipmentItemView(
                item.VariantId,
                item.Variant?.Name ?? "-",
                item.Quantity,
                item.Volume))
            .ToList();

        var transportUnits = shipment.TransportUnits
            .Select(unit => new ShipmentTransportUnitView(
                unit.Mode,
                unit.Identifier,
                unit.Count))
            .ToList();

        CustomsInfoView? customs = null;
        if (shipment.CustomsProcess is not null)
        {
            customs = new CustomsInfoView(
                shipment.CustomsProcess.Status,
                shipment.CustomsProcess.StartedAt,
                shipment.CustomsProcess.CompletedAt,
                shipment.CustomsProcess.DocumentNumber,
                shipment.CustomsProcess.Notes);
        }

        Shipment = new ShipmentDetailView(
            shipment.Id,
            shipment.ReferenceNumber,
            shipment.Supplier?.Name ?? "-",
            shipment.Customer?.Name,
            shipment.Status,
            shipment.ShipmentDate,
            shipment.EstimatedArrival,
            shipment.LoadingPort,
            shipment.DischargePort,
            shipment.Notes,
            stages,
            customs,
            items,
            transportUnits);

        return Page();
    }

    public sealed record ShipmentDetailView(
        Guid Id,
        string ReferenceNumber,
        string SupplierName,
        string? CustomerName,
        ShipmentStatus Status,
        DateTime ShipmentDate,
        DateTime? EstimatedArrival,
        string? LoadingPort,
        string? DischargePort,
        string? Notes,
        IReadOnlyList<ShipmentStageView> Stages,
        CustomsInfoView? Customs,
        IReadOnlyList<ShipmentItemView> Items,
        IReadOnlyList<ShipmentTransportUnitView> TransportUnits);

    public sealed record ShipmentStageView(
        ShipmentStatus Status,
        DateTime StartedAt,
        DateTime? CompletedAt,
        string? Notes);

    public sealed record ShipmentItemView(
        Guid VariantId,
        string VariantName,
        decimal Quantity,
        decimal Volume);

    public sealed record ShipmentTransportUnitView(
        TransportMode Mode,
        string Identifier,
        int Count);

    public sealed record CustomsInfoView(
        CustomsStatus Status,
        DateTime StartedAt,
        DateTime? CompletedAt,
        string? DocumentNumber,
        string? Notes);
}


