using CRM.Application.Common;
using CRM.Domain.Shipments;
using CRM.Domain.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Shipments;

public class ShipmentService : IShipmentService
{
    private readonly IRepository<Shipment> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentService(IRepository<Shipment> repository, IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ShipmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments.AsNoTracking()
            .Include(s => s.Supplier).Include(s => s.Customer).Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment == null)
        {
            return null;
        }

        return new ShipmentDto(shipment.Id, shipment.SupplierId, shipment.Supplier?.Name ?? "-",
            shipment.CustomerId, shipment.Customer?.Name, shipment.ReferenceNumber, shipment.ShipmentDate,
            shipment.EstimatedArrival, shipment.Status, shipment.LoadingPort, shipment.DischargePort,
            shipment.Notes, shipment.CreatedAt);
    }

    public async Task<IReadOnlyList<ShipmentListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var shipments = await _context.Shipments.AsNoTracking().Include(s => s.Supplier)
            .Include(s => s.Customer).Include(s => s.Stages).OrderByDescending(s => s.ShipmentDate).ToListAsync(cancellationToken);

        return shipments.Select(s =>
        {
            var latestStage = s.Stages.OrderByDescending(stage => stage.StartedAt).FirstOrDefault();

            return new ShipmentListItemDto(s.Id, s.ReferenceNumber, s.Supplier?.Name ?? "-",
                s.Customer?.Name ?? "-", s.Status, s.ShipmentDate, s.EstimatedArrival,
                latestStage?.StartedAt, latestStage?.Notes);
        }).ToList();
    }

    public async Task<Guid> CreateAsync(CreateShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var shipmentDate = DateTime.SpecifyKind(request.ShipmentDate, DateTimeKind.Utc);
        DateTime? estimatedArrival = request.EstimatedArrival.HasValue
            ? DateTime.SpecifyKind(request.EstimatedArrival.Value, DateTimeKind.Utc)
            : null;
        var stageStartedAt = DateTime.SpecifyKind(request.StageStartedAt, DateTimeKind.Utc);
        DateTime? stageCompletedAt = request.StageCompletedAt.HasValue
            ? DateTime.SpecifyKind(request.StageCompletedAt.Value, DateTimeKind.Utc)
            : null;

        var shipment = new Shipment(Guid.NewGuid(), request.SupplierId,
            request.ReferenceNumber.Trim(), shipmentDate, request.Status, request.CustomerId);

        shipment.Update(shipmentDate, estimatedArrival,
            request.Status, request.LoadingPort, request.DischargePort, request.Notes, request.CustomerId);

        shipment.SetOrUpdateStage(request.Status, stageStartedAt, stageCompletedAt, request.StageNotes);

        await _repository.AddAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return shipment.Id;
    }

    public async Task UpdateAsync(UpdateShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments.Include(s => s.Stages).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (shipment == null)
        {
            throw new InvalidOperationException($"Shipment with id {request.Id} not found.");
        }

        shipment.ReassignSupplier(request.SupplierId);
        shipment.ReassignCustomer(request.CustomerId);

        var shipmentDate = DateTime.SpecifyKind(request.ShipmentDate, DateTimeKind.Utc);
        DateTime? estimatedArrival = request.EstimatedArrival.HasValue
            ? DateTime.SpecifyKind(request.EstimatedArrival.Value, DateTimeKind.Utc)
            : null;

        shipment.Update(shipmentDate, estimatedArrival, request.Status, request.LoadingPort,
            request.DischargePort, request.Notes, request.CustomerId);

        var stageStartedAt = DateTime.SpecifyKind(request.StageStartedAt, DateTimeKind.Utc);
        DateTime? stageCompletedAt = request.StageCompletedAt.HasValue
            ? DateTime.SpecifyKind(request.StageCompletedAt.Value, DateTimeKind.Utc)
            : null;

        shipment.SetOrUpdateStage(request.Status, stageStartedAt, stageCompletedAt, request.StageNotes);

        await _repository.UpdateAsync(shipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShipmentDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments.AsNoTracking().Include(s => s.Supplier)
            .Include(s => s.Customer).Include(s => s.Stages).Include(s => s.CustomsProcess).Include(s => s.Items)
                .ThenInclude(i => i.Variant).Include(s => s.TransportUnits).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment == null)
        {
            return null;
        }

        var stages = shipment.Stages.OrderBy(stage => stage.StartedAt).Select(stage => new ShipmentStageDto(
                stage.Status, stage.StartedAt, stage.CompletedAt, stage.Notes)).ToList();

        var items = shipment.Items.Select(item => new ShipmentItemDto(
                item.VariantId, item.Variant?.Name ?? "-", item.Quantity, item.Volume)).ToList();

        var transportUnits = shipment.TransportUnits
            .Select(unit => new ShipmentTransportUnitDto(
                unit.Mode,
                unit.Identifier,
                unit.Count))
            .ToList();

        CustomsInfoDto? customs = null;
        if (shipment.CustomsProcess is not null)
        {
            customs = new CustomsInfoDto(shipment.CustomsProcess.Status, shipment.CustomsProcess.StartedAt,
                shipment.CustomsProcess.CompletedAt, shipment.CustomsProcess.DocumentNumber, shipment.CustomsProcess.Notes);
        }

        return new ShipmentDetailsDto(shipment.Id, shipment.ReferenceNumber, shipment.Supplier?.Name ?? "-",
            shipment.Customer?.Name, shipment.Status, shipment.ShipmentDate, shipment.EstimatedArrival,
            shipment.LoadingPort, shipment.DischargePort, shipment.Notes, stages,
            customs, items, transportUnits);
    }

    public async Task<ShipmentDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        var shipments = await GetAllAsync(cancellationToken);

        var totalShipments = shipments.Count;
        var deliveredShipments = shipments.Count(s => s.Status == ShipmentStatus.DeliveredToDealer);
        var activeShipments = shipments.Count(s => s.Status != ShipmentStatus.DeliveredToDealer && s.Status != ShipmentStatus.Cancelled);
        var customsShipments = shipments.Count(s => s.Status == ShipmentStatus.InCustoms);

        var statusSummaries = shipments.GroupBy(s => s.Status).Select(group => new StatusSummary(group.Key, group.Count()))
            .OrderByDescending(summary => summary.Count).ThenBy(summary => summary.Status).ToList();

        return new ShipmentDashboardData(shipments, totalShipments,
            activeShipments, deliveredShipments, customsShipments, statusSummaries);
    }
}

