using CRM.Application.Common;
using CRM.Application.Common.Caching;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IApplicationDbContext context, 
        ICacheService cacheService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<AnalyticsService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<AnalyticsData> GetOperationsAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.AnalyticsOperations,
            async () => await LoadOperationsAnalyticsAsync(cancellationToken),
            TimeSpan.FromMinutes(5),
            cancellationToken);
    }

    private async Task<AnalyticsData> LoadOperationsAnalyticsAsync(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var ninetyDaysAgo = nowUtc.AddDays(-90);
        var thirtyDaysAgo = nowUtc.AddDays(-30);

        var shipments = await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Stages)
            .Include(s => s.Customer)
            .Include(s => s.Supplier)
            .Where(s => s.CreatedAt >= ninetyDaysAgo || s.Status != ShipmentStatus.DeliveredToDealer)
            .ToListAsync(cancellationToken);

        var unloadings = await _context.WarehouseUnloadings
            .AsNoTracking()
            .Include(u => u.Warehouse)
            .Where(u => u.UnloadedAt >= thirtyDaysAgo)
            .ToListAsync(cancellationToken);

        var totalShipments = shipments.Count;
        var activeShipments = shipments.Count(s => s.Status != ShipmentStatus.DeliveredToDealer && s.Status != ShipmentStatus.Cancelled);

        var deliveredWithin30 = shipments
            .Select(shipment => new
            {
                Shipment = shipment,
                DeliveredStage = shipment.Stages?.FirstOrDefault(stage => stage.Status == ShipmentStatus.DeliveredToDealer && stage.CompletedAt.HasValue)
            })
            .Where(x => x.DeliveredStage?.CompletedAt >= thirtyDaysAgo)
            .Select(x => (x.DeliveredStage!.CompletedAt!.Value - x.Shipment.ShipmentDate).TotalDays)
            .ToList();

        var avgTransitDays = deliveredWithin30.Count > 0 ? deliveredWithin30.Average() : 0d;

        var customsDurations = shipments
            .Where(s => s.Status == ShipmentStatus.InCustoms)
            .Select(shipment =>
            {
                var customsStage = shipment.Stages?.FirstOrDefault(stage => stage.Status == ShipmentStatus.InCustoms);
                var anchor = customsStage?.StartedAt ?? shipment.LastModifiedAt ?? shipment.CreatedAt;
                return (nowUtc - anchor).TotalDays;
            })
            .ToList();

        var customsBacklogDays = customsDurations.Count > 0 ? customsDurations.Average() : 0d;

        var summary = new OperationsSummary(
            totalShipments,
            activeShipments,
            Math.Round(avgTransitDays, 1),
            Math.Round(customsBacklogDays, 1));

        var statusBreakdown = shipments
            .GroupBy(shipment => shipment.Status)
            .Select(group => new StatusBreakdownItem(group.Key, group.Count()))
            .OrderByDescending(item => item.Count)
            .ToList();

        const int weeks = 8;
        var weekStart = StartOfWeek(nowUtc, DayOfWeek.Monday).AddDays(-(weeks - 1) * 7);
        var deliveredStages = shipments
            .SelectMany(shipment => shipment.Stages ?? Array.Empty<Domain.Shipments.ShipmentStage>())
            .Where(stage => stage.Status == ShipmentStatus.DeliveredToDealer && stage.CompletedAt.HasValue && stage.CompletedAt.Value >= weekStart)
            .Select(stage => stage.CompletedAt!.Value);

        var deliveredByWeek = deliveredStages
            .GroupBy(date => StartOfWeek(date, DayOfWeek.Monday))
            .ToDictionary(group => group.Key, group => group.Count());

        var trendBuckets = Enumerable.Range(0, weeks)
            .Select(offset => StartOfWeek(nowUtc, DayOfWeek.Monday).AddDays(-(weeks - 1 - offset) * 7))
            .ToList();

        var completionTrend = trendBuckets
            .Select(period => new CompletionTrendPoint(period, deliveredByWeek.TryGetValue(period, out var count) ? count : 0))
            .ToList();

        var warehouseThroughput = unloadings
            .GroupBy(unloading => unloading.Warehouse?.Name ?? _localizer["OperationsAnalytics_UnassignedWarehouse"].Value)
            .Select(group => new WarehouseThroughputItem(
                group.Key,
                group.Count(),
                group.Sum(x => x.UnloadedVolume)))
            .OrderByDescending(item => item.UnloadingCount)
            .ThenByDescending(item => item.TotalVolume)
            .Take(8)
            .ToList();

        var delayShipments = shipments
            .Where(s => s.Status != ShipmentStatus.DeliveredToDealer && s.Status != ShipmentStatus.Cancelled)
            .Select(shipment =>
            {
                var lastStage = shipment.Stages?.OrderByDescending(stage => stage.CompletedAt ?? stage.StartedAt).FirstOrDefault();
                var anchor = lastStage?.StartedAt ?? shipment.ShipmentDate;
                var days = (nowUtc - anchor).TotalDays;
                var customerDisplay = shipment.Customer?.Name ?? shipment.Supplier?.Name ?? string.Empty;
                return new DelayShipmentItem(
                    shipment.ReferenceNumber,
                    customerDisplay,
                    shipment.Status,
                    Math.Round(days, 1));
            })
            .OrderByDescending(item => item.DaysInStage)
            .ThenBy(item => item.ReferenceNumber)
            .Take(6)
            .ToList();

        return new AnalyticsData
        {
            Summary = summary,
            StatusBreakdown = statusBreakdown,
            CompletionTrend = completionTrend,
            WarehouseThroughput = warehouseThroughput,
            DelayShipments = delayShipments
        };
    }

    private static DateTime StartOfWeek(DateTime dateTime, DayOfWeek firstDay) =>
        dateTime.Date.AddDays(-(((int)dateTime.DayOfWeek - (int)firstDay + 7) % 7));
}

