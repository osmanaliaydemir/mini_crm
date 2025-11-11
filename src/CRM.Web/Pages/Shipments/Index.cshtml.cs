using System.Linq;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Shipments;

public class IndexModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public IndexModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IList<ShipmentListItem> Shipments { get; private set; } = new List<ShipmentListItem>();
    public int TotalShipments { get; private set; }
    public int ActiveShipments { get; private set; }
    public int DeliveredShipments { get; private set; }
    public int CustomsShipments { get; private set; }
    public IReadOnlyList<StatusSummary> StatusSummaries { get; private set; } = Array.Empty<StatusSummary>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var shipments = await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Supplier)
            .Include(s => s.Customer)
            .Include(s => s.Stages)
            .OrderByDescending(s => s.ShipmentDate)
            .ToListAsync(cancellationToken);

        Shipments = shipments.Select(s =>
        {
            var latestStage = s.Stages
                .OrderByDescending(stage => stage.StartedAt)
                .FirstOrDefault();

            return new ShipmentListItem(
                s.Id,
                s.ReferenceNumber,
                s.Supplier?.Name ?? "-",
                s.Customer?.Name ?? "-",
                s.Status,
                s.ShipmentDate,
                s.EstimatedArrival,
                latestStage?.StartedAt,
                latestStage?.Notes);
        }).ToList();

        TotalShipments = Shipments.Count;
        DeliveredShipments = Shipments.Count(s => s.Status == ShipmentStatus.DeliveredToDealer);
        ActiveShipments = Shipments.Count(s => s.Status != ShipmentStatus.DeliveredToDealer && s.Status != ShipmentStatus.Cancelled);
        CustomsShipments = Shipments.Count(s => s.Status == ShipmentStatus.InCustoms);

        StatusSummaries = Shipments
            .GroupBy(s => s.Status)
            .Select(group => new StatusSummary(group.Key, group.Count()))
            .OrderByDescending(summary => summary.Count)
            .ThenBy(summary => summary.Status)
            .ToList();
    }

    public string GetStatusBadgeClass(ShipmentStatus status) =>
        status switch
        {
            ShipmentStatus.DeliveredToDealer => "bg-success-subtle text-success",
            ShipmentStatus.Cancelled => "bg-danger-subtle text-danger",
            ShipmentStatus.InCustoms => "bg-warning-subtle text-warning",
            ShipmentStatus.OnVessel or ShipmentStatus.OnTrain or ShipmentStatus.OnTruck => "bg-primary-subtle text-primary",
            ShipmentStatus.InWarehouse => "bg-info-subtle text-info",
            ShipmentStatus.ProductionStarted or ShipmentStatus.Ordered => "bg-secondary-subtle text-secondary",
            _ => "bg-light text-dark"
        };

    public sealed record ShipmentListItem(
        Guid Id,
        string ReferenceNumber,
        string SupplierName,
        string CustomerName,
        ShipmentStatus Status,
        DateTime ShipmentDate,
        DateTime? EstimatedArrival,
        DateTime? LastStageUpdate,
        string? LastStageNotes);

    public sealed record StatusSummary(ShipmentStatus Status, int Count);
}


