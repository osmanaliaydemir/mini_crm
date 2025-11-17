using CRM.Application.Common;
using CRM.Application.Common.Caching;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Application.Notifications.Automation;
using CRM.Domain.Notifications;
using CRM.Domain.Shipments;
using CRM.Domain.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Shipments;

public class ShipmentService : IShipmentService
{
    private readonly IRepository<Shipment> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ShipmentService> _logger;
    private readonly IEmailAutomationService _emailAutomationService;

    public ShipmentService(
        IRepository<Shipment> repository, 
        IApplicationDbContext context, 
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ShipmentService> logger,
        IEmailAutomationService emailAutomationService)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
        _emailAutomationService = emailAutomationService;
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
            shipment.Notes, shipment.CreatedAt, shipment.RowVersion);
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

    public async Task<PagedResult<ShipmentListItemDto>> GetAllPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.Shipments.AsNoTracking()
            .Include(s => s.Supplier)
            .Include(s => s.Customer)
            .Include(s => s.Stages)
            .OrderByDescending(s => s.ShipmentDate);

        var pagedResult = await query.ToPagedResultAsync(pagination, cancellationToken);

        var items = pagedResult.Items.Select(s =>
        {
            var latestStage = s.Stages.OrderByDescending(stage => stage.StartedAt).FirstOrDefault();

            return new ShipmentListItemDto(s.Id, s.ReferenceNumber, s.Supplier?.Name ?? "-",
                s.Customer?.Name ?? "-", s.Status, s.ShipmentDate, s.EstimatedArrival,
                latestStage?.StartedAt, latestStage?.Notes);
        }).ToList();

        return new PagedResult<ShipmentListItemDto>(items, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
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

        // Cache invalidation - Shipment ve ana dashboard cache'lerini temizle
        await _cacheService.RemoveAsync(CacheKeys.ShipmentDashboard, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.AnalyticsOperations, cancellationToken);

        return shipment.Id;
    }

    public async Task UpdateAsync(UpdateShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Stages)
            .Include(s => s.Supplier)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (shipment == null)
        {
            throw new NotFoundException(nameof(Shipment), request.Id);
        }

        // Set RowVersion for optimistic concurrency control
        shipment.RowVersion = request.RowVersion;

        var previousStatus = shipment.Status;

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

        // Cache invalidation - Shipment ve ana dashboard cache'lerini temizle
        await _cacheService.RemoveAsync(CacheKeys.ShipmentDashboard, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.AnalyticsOperations, cancellationToken);

        if (previousStatus != shipment.Status)
        {
            await SendShipmentStatusChangedNotificationAsync(shipment, previousStatus, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.StageNotes))
        {
            await SendShipmentNoteNotificationAsync(shipment, request.StageNotes!, cancellationToken);
        }
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
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.ShipmentDashboard,
            async () => await LoadShipmentDashboardDataAsync(cancellationToken),
            TimeSpan.FromMinutes(5),
            cancellationToken);
    }

    private async Task<ShipmentDashboardData> LoadShipmentDashboardDataAsync(CancellationToken cancellationToken)
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

    private async Task SendShipmentStatusChangedNotificationAsync(Shipment shipment, ShipmentStatus previousStatus, CancellationToken cancellationToken)
    {
        var subject = $"Sevkiyat Durum Güncellemesi - {shipment.ReferenceNumber}";
        var content = $@"<p><strong>Referans:</strong> {shipment.ReferenceNumber}</p>
                         <p><strong>Müşteri:</strong> {shipment.Customer?.Name ?? "-"} | <strong>Tedarikçi:</strong> {shipment.Supplier?.Name ?? "-"}</p>
                         <p><strong>Durum:</strong> {previousStatus} → {shipment.Status}</p>
                         <p><strong>Varış:</strong> {(shipment.EstimatedArrival?.ToString("dd.MM.yyyy") ?? "-")}</p>
                         <p><strong>Notlar:</strong> {shipment.Notes ?? "-"}</p>";

        var context = new EmailAutomationEventContext
        {
            ResourceType = EmailResourceType.Shipment,
            TriggerType = EmailTriggerType.ShipmentStatusChanged,
            RelatedEntityId = shipment.Id,
            TemplateKey = "GenericNotification",
            Subject = subject,
            Placeholders = new Dictionary<string, string>
            {
                ["Title"] = "Sevkiyat Durumu Güncellendi",
                ["Description"] = $"{shipment.ReferenceNumber} numaralı sevkiyatın durumu güncellendi.",
                ["Content"] = content
            }
        };

        await _emailAutomationService.HandleEventAsync(context, cancellationToken);
    }

    private async Task SendShipmentNoteNotificationAsync(Shipment shipment, string note, CancellationToken cancellationToken)
    {
        var context = new EmailAutomationEventContext
        {
            ResourceType = EmailResourceType.Shipment,
            TriggerType = EmailTriggerType.ShipmentNoteAdded,
            RelatedEntityId = shipment.Id,
            TemplateKey = "GenericNotification",
            Subject = $"Sevkiyat Notu Güncellendi - {shipment.ReferenceNumber}",
            Placeholders = new Dictionary<string, string>
            {
                ["Title"] = "Sevkiyat Notu Eklendi",
                ["Description"] = $"{shipment.ReferenceNumber} numaralı sevkiyata yeni bir not eklendi.",
                ["Content"] = $"<p>{note}</p>"
            }
        };

        await _emailAutomationService.HandleEventAsync(context, cancellationToken);
    }
}

