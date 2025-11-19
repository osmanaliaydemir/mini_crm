using CRM.Application.Common;
using CRM.Application.Common.Caching;
using CRM.Domain.Enums;
using CRM.Domain.Finance;
using CRM.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DashboardService> _logger;

    private const string UnspecifiedLabel = "UNSPECIFIED";

    public DashboardService(
        IApplicationDbContext context, 
        ICacheService cacheService,
        ILogger<DashboardService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<DashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.DashboardData,
            async () => await LoadDashboardDataAsync(cancellationToken),
            TimeSpan.FromMinutes(5),
            cancellationToken);
    }

    private async Task<DashboardData> LoadDashboardDataAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthBuckets = BuildMonthSeries(now, 6);
        var sixMonthsStart = monthBuckets.First();
        var thirtyDaysAgo = now.AddDays(-30);
        var activityThreshold = now.AddDays(-90);
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var todayEnd = todayStart.AddDays(1);

        // Execute queries sequentially to avoid DbContext concurrency issues
        // EF Core DbContext is not thread-safe and cannot handle concurrent operations
        // However, queries are still optimized with projections and AsNoTracking()
        var shipments = await GetShipmentsForDashboardAsync(sixMonthsStart, activityThreshold, cancellationToken);
        
        var supplierCountries = await _context.Suppliers
            .AsNoTracking()
            .Select(s => s.Country)
            .ToListAsync(cancellationToken);
        
        var totalCustomers = await _context.Customers
            .AsNoTracking()
            .CountAsync(cancellationToken);
        
        var totalWarehouses = await _context.Warehouses
            .AsNoTracking()
            .CountAsync(cancellationToken);
        
        var warehouseUnloadings = await _context.WarehouseUnloadings
            .AsNoTracking()
            .Where(w => w.UnloadedAt >= sixMonthsStart)
            .Select(w => new WarehouseUnloadingProjection(w.UnloadedAt, w.UnloadedVolume))
            .ToListAsync(cancellationToken);
        
        var interactions = await _context.CustomerInteractions
            .AsNoTracking()
            .Where(i => i.InteractionDate >= sixMonthsStart)
            .Select(i => i.InteractionDate)
            .ToListAsync(cancellationToken);
        
        var cashTransactions = await _context.CashTransactions
            .AsNoTracking()
            .Where(t => t.TransactionDate >= sixMonthsStart)
            .Select(t => new CashTransactionProjection(t.TransactionDate, t.TransactionType, t.Amount))
            .ToListAsync(cancellationToken);
        
        var todayTasksList = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Status != Domain.Tasks.TaskStatus.Completed &&
                        t.Status != Domain.Tasks.TaskStatus.Cancelled &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value <= todayEnd)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .Take(10)
            .ToListAsync(cancellationToken);
        
        var totalShipments = await _context.Shipments
            .AsNoTracking()
            .CountAsync(cancellationToken);
        
        var activeShipments = await _context.Shipments
            .AsNoTracking()
            .CountAsync(s => s.Status != ShipmentStatus.DeliveredToDealer && s.Status != ShipmentStatus.Cancelled, cancellationToken);
        
        var shipmentsInCustoms = await _context.Shipments
            .AsNoTracking()
            .CountAsync(s => s.Status == ShipmentStatus.InCustoms, cancellationToken);

        var deliveredLast30 = shipments.Count(s =>
            s.Status == ShipmentStatus.DeliveredToDealer &&
            (s.Stages.Any(stage => stage.Status == ShipmentStatus.DeliveredToDealer &&
                                   ((stage.CompletedAt ?? stage.StartedAt) >= thirtyDaysAgo)) ||
             (s.LastModifiedAt ?? s.CreatedAt) >= thirtyDaysAgo));

        var shipmentStatusSummaries = shipments
            .GroupBy(s => s.Status)
            .Select(group => new StatusSummary(group.Key, group.Count()))
            .OrderByDescending(summary => summary.Count)
            .ToList();

        var monthDict = shipments
            .Where(s => s.ShipmentDate >= sixMonthsStart)
            .GroupBy(s => new DateTime(s.ShipmentDate.Year, s.ShipmentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToDictionary(g => g.Key, g => g.Count());

        var shipmentMonthlyTrend = monthBuckets
            .Select(month => new TimeSeriesPoint(month, monthDict.TryGetValue(month, out var value) ? value : 0))
            .ToList();

        var supplierCountryBreakdown = supplierCountries
            .GroupBy(NormalizeSupplierCountry, StringComparer.OrdinalIgnoreCase)
            .Select(group => new CategoryPoint(group.Key, group.Count()))
            .OrderByDescending(point => point.Value)
            .ThenBy(point => point.Label)
            .Take(5)
            .ToList();

        var warehouseVolumeDict = warehouseUnloadings
            .GroupBy(u => new DateTime(u.UnloadedAt.Year, u.UnloadedAt.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.UnloadedVolume));

        var warehouseVolumeTrend = monthBuckets
            .Select(month => new TimeSeriesPoint(month, warehouseVolumeDict.TryGetValue(month, out var volume) ? volume : 0))
            .ToList();

        var cashFlowDict = cashTransactions
            .GroupBy(t => new DateTime(t.TransactionDate.Year, t.TransactionDate.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Income = g.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount)
                });

        var cashFlowTrend = monthBuckets
            .Select(month => cashFlowDict.TryGetValue(month, out var values)
                ? new CashFlowPoint(month, values.Income, values.Expense)
                : new CashFlowPoint(month, 0, 0))
            .ToList();

        var interactionDict = interactions
            .GroupBy(i => new DateTime(i.Year, i.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToDictionary(g => g.Key, g => g.Count());

        var customerInteractionTrend = monthBuckets
            .Select(month => new TimeSeriesPoint(month, interactionDict.TryGetValue(month, out var count) ? count : 0))
            .ToList();

        var warehouseUnloadingsLast30 = warehouseUnloadings.Count(u => u.UnloadedAt >= thirtyDaysAgo);
        var cashNetLast30 = cashTransactions
            .Where(t => t.TransactionDate >= thirtyDaysAgo)
            .Sum(t => t.TransactionType == CashTransactionType.Income ? t.Amount : -t.Amount);
        var interactionsLast30 = interactions.Count(i => i >= thirtyDaysAgo);

        var summary = new DashboardSummary(
            TotalShipments: totalShipments,
            ActiveShipments: activeShipments,
            ShipmentsInCustoms: shipmentsInCustoms,
            DeliveredLast30Days: deliveredLast30,
            TotalSuppliers: supplierCountries.Count,
            TotalCustomers: totalCustomers,
            TotalWarehouses: totalWarehouses,
            WarehouseUnloadingsLast30: warehouseUnloadingsLast30,
            CashNetLast30: cashNetLast30,
            CustomerInteractionsLast30: interactionsLast30);

        var nowUtc = DateTime.UtcNow;
        var activityFeed = new Dictionary<string, IReadOnlyList<ActivityEvent>>
        {
            ["7d"] = BuildActivityEvents(shipments, nowUtc, 7),
            ["30d"] = BuildActivityEvents(shipments, nowUtc, 30),
            ["90d"] = BuildActivityEvents(shipments, nowUtc, 90)
        };

        // User names will be loaded in the view layer using UserManager
        var taskSummaries = todayTasksList.Select(t => new TaskSummary(
            t.Id,
            t.Title,
            t.Status,
            t.Priority,
            t.DueDate,
            t.AssignedToUserId,
            null // User names will be loaded in the view layer if needed
        )).ToList();

        return new DashboardData
        {
            Summary = summary,
            ShipmentStatusSummaries = shipmentStatusSummaries,
            ShipmentMonthlyTrend = shipmentMonthlyTrend,
            SupplierCountryBreakdown = supplierCountryBreakdown,
            WarehouseVolumeTrend = warehouseVolumeTrend,
            CashFlowTrend = cashFlowTrend,
            CustomerInteractionTrend = customerInteractionTrend,
            ActivityFeed = activityFeed,
            TodayTasks = taskSummaries
        };
    }

    private static IReadOnlyList<DateTime> BuildMonthSeries(DateTime referenceUtc, int months)
    {
        var firstOfCurrentMonth = new DateTime(referenceUtc.Year, referenceUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var start = firstOfCurrentMonth.AddMonths(-(months - 1));
        var list = new List<DateTime>(months);
        for (var offset = 0; offset < months; offset++)
        {
            list.Add(start.AddMonths(offset));
        }

        return list;
    }

    private static string NormalizeSupplierCountry(string? country) =>
        string.IsNullOrWhiteSpace(country) ? UnspecifiedLabel : country.Trim();

    private static IReadOnlyList<ActivityEvent> BuildActivityEvents(IEnumerable<ShipmentDashboardProjection> shipments, DateTime referenceUtc, int dayWindow)
    {
        var threshold = referenceUtc.AddDays(-dayWindow);

        return shipments
            .Select(shipment =>
            {
                var latestStage = shipment.Stages
                    ?.OrderByDescending(stage => stage.CompletedAt ?? stage.StartedAt)
                    .FirstOrDefault();

                var stageMoment = latestStage != null ? (latestStage.CompletedAt ?? latestStage.StartedAt) : (DateTime?)null;
                var occurredAt = shipment.LastModifiedAt
                                  ?? stageMoment
                                  ?? shipment.CreatedAt;

                var customerDisplay = !string.IsNullOrWhiteSpace(shipment.CustomerName)
                    ? shipment.CustomerName
                    : shipment.CustomerId.HasValue
                        ? shipment.CustomerId.Value.ToString("N")
                        : null;

                return new ActivityEvent(
                    occurredAt,
                    shipment.Status,
                    shipment.ReferenceNumber,
                    customerDisplay);
            })
            .Where(activity => activity.OccurredAt >= threshold)
            .OrderByDescending(activity => activity.OccurredAt)
            .Take(6)
            .ToList();
    }

    private Task<List<ShipmentDashboardProjection>> GetShipmentsForDashboardAsync(
        DateTime sixMonthsStart,
        DateTime activityThreshold,
        CancellationToken cancellationToken)
    {
        return _context.Shipments
            .AsNoTracking()
            .Where(s =>
                s.ShipmentDate >= sixMonthsStart ||
                (s.LastModifiedAt ?? s.CreatedAt) >= activityThreshold ||
                s.Stages.Any(stage => (stage.CompletedAt ?? stage.StartedAt) >= activityThreshold))
            .Select(s => new ShipmentDashboardProjection(
                s.Id,
                s.ReferenceNumber,
                s.Status,
                s.ShipmentDate,
                s.CreatedAt,
                s.LastModifiedAt,
                s.CustomerId,
                s.Customer != null ? s.Customer.Name : null,
                s.Stages.Select(stage => new ShipmentStageProjection(
                    stage.Status,
                    stage.StartedAt,
                    stage.CompletedAt)).ToList()))
            .ToListAsync(cancellationToken);
    }

    private sealed record ShipmentDashboardProjection(
        Guid Id,
        string ReferenceNumber,
        ShipmentStatus Status,
        DateTime ShipmentDate,
        DateTime CreatedAt,
        DateTime? LastModifiedAt,
        Guid? CustomerId,
        string? CustomerName,
        IReadOnlyList<ShipmentStageProjection> Stages);

    private sealed record ShipmentStageProjection(
        ShipmentStatus Status,
        DateTime StartedAt,
        DateTime? CompletedAt);

    private sealed record WarehouseUnloadingProjection(DateTime UnloadedAt, decimal UnloadedVolume);

    private sealed record CashTransactionProjection(
        DateTime TransactionDate,
        CashTransactionType TransactionType,
        decimal Amount);
}

