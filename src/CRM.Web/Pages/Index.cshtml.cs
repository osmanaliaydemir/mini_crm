using CRM.Domain.Enums;
using CRM.Domain.Finance;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly CRMDbContext _dbContext;

    private const string UnspecifiedLabel = "UNSPECIFIED";

    public DashboardSummary Summary { get; private set; } = DashboardSummary.Empty;
    public IReadOnlyList<StatusSummary> ShipmentStatusSummaries { get; private set; } = Array.Empty<StatusSummary>();
    public IReadOnlyList<TimeSeriesPoint> ShipmentMonthlyTrend { get; private set; } = Array.Empty<TimeSeriesPoint>();
    public IReadOnlyList<CategoryPoint> SupplierCountryBreakdown { get; private set; } = Array.Empty<CategoryPoint>();
    public IReadOnlyList<TimeSeriesPoint> WarehouseVolumeTrend { get; private set; } = Array.Empty<TimeSeriesPoint>();
    public IReadOnlyList<TimeSeriesPoint> CashFlowTrend { get; private set; } = Array.Empty<TimeSeriesPoint>();
    public IReadOnlyList<TimeSeriesPoint> CustomerInteractionTrend { get; private set; } = Array.Empty<TimeSeriesPoint>();

    public static string SupplierUnspecifiedToken => UnspecifiedLabel;

    public IndexModel(ILogger<IndexModel> logger, CRMDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthBuckets = BuildMonthSeries(now, 6);
        var sixMonthsStart = monthBuckets.First();
        var thirtyDaysAgo = now.AddDays(-30);

        var shipments = await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Stages)
            .ToListAsync(cancellationToken);

        var suppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalCustomers = await _dbContext.Customers
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalWarehouses = await _dbContext.Warehouses
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var warehouseUnloadings = await _dbContext.WarehouseUnloadings
            .AsNoTracking()
            .Where(w => w.UnloadedAt >= sixMonthsStart)
            .ToListAsync(cancellationToken);

        var interactions = await _dbContext.CustomerInteractions
            .AsNoTracking()
            .Where(i => i.InteractionDate >= sixMonthsStart)
            .ToListAsync(cancellationToken);

        var cashTransactions = await _dbContext.CashTransactions
            .AsNoTracking()
            .Where(t => t.TransactionDate >= sixMonthsStart)
            .ToListAsync(cancellationToken);

        var totalShipments = shipments.Count;
        var activeShipments = shipments.Count(s => s.Status != ShipmentStatus.DeliveredToDealer && s.Status != ShipmentStatus.Cancelled);
        var shipmentsInCustoms = shipments.Count(s => s.Status == ShipmentStatus.InCustoms);

        var deliveredLast30 = shipments.Count(s =>
            s.Status == ShipmentStatus.DeliveredToDealer &&
            (s.Stages.Any(stage => stage.Status == ShipmentStatus.DeliveredToDealer &&
                                   ((stage.CompletedAt ?? stage.StartedAt) >= thirtyDaysAgo)) ||
             (s.LastModifiedAt ?? s.CreatedAt) >= thirtyDaysAgo));

        ShipmentStatusSummaries = shipments
            .GroupBy(s => s.Status)
            .Select(group => new StatusSummary(group.Key, group.Count()))
            .OrderByDescending(summary => summary.Count)
            .ToList();

        var monthDict = shipments
            .Where(s => s.ShipmentDate >= sixMonthsStart)
            .GroupBy(s => new DateTime(s.ShipmentDate.Year, s.ShipmentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToDictionary(g => g.Key, g => g.Count());

        ShipmentMonthlyTrend = monthBuckets
            .Select(month => new TimeSeriesPoint(month, monthDict.TryGetValue(month, out var value) ? value : 0))
            .ToList();

        SupplierCountryBreakdown = suppliers
            .GroupBy(s => NormalizeSupplierCountry(s.Country), StringComparer.OrdinalIgnoreCase)
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

        WarehouseVolumeTrend = monthBuckets
            .Select(month => new TimeSeriesPoint(month, warehouseVolumeDict.TryGetValue(month, out var volume) ? volume : 0))
            .ToList();

        var cashFlowDict = cashTransactions
            .GroupBy(t => new DateTime(t.TransactionDate.Year, t.TransactionDate.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(t => t.TransactionType == CashTransactionType.Income ? t.Amount : -t.Amount));

        CashFlowTrend = monthBuckets
            .Select(month => new TimeSeriesPoint(month, cashFlowDict.TryGetValue(month, out var net) ? net : 0))
            .ToList();

        var interactionDict = interactions
            .GroupBy(i => new DateTime(i.InteractionDate.Year, i.InteractionDate.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToDictionary(g => g.Key, g => g.Count());

        CustomerInteractionTrend = monthBuckets
            .Select(month => new TimeSeriesPoint(month, interactionDict.TryGetValue(month, out var count) ? count : 0))
            .ToList();

        var warehouseUnloadingsLast30 = warehouseUnloadings.Count(u => u.UnloadedAt >= thirtyDaysAgo);
        var cashNetLast30 = cashTransactions
            .Where(t => t.TransactionDate >= thirtyDaysAgo)
            .Sum(t => t.TransactionType == CashTransactionType.Income ? t.Amount : -t.Amount);
        var interactionsLast30 = interactions.Count(i => i.InteractionDate >= thirtyDaysAgo);

        Summary = new DashboardSummary(
            TotalShipments: totalShipments,
            ActiveShipments: activeShipments,
            ShipmentsInCustoms: shipmentsInCustoms,
            DeliveredLast30Days: deliveredLast30,
            TotalSuppliers: suppliers.Count,
            TotalCustomers: totalCustomers,
            TotalWarehouses: totalWarehouses,
            WarehouseUnloadingsLast30: warehouseUnloadingsLast30,
            CashNetLast30: cashNetLast30,
            CustomerInteractionsLast30: interactionsLast30);
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

    public sealed record StatusSummary(ShipmentStatus Status, int Count);

    public sealed record TimeSeriesPoint(DateTime PeriodStart, decimal Value);

    public sealed record CategoryPoint(string Label, int Value);

    public sealed record DashboardSummary(
        int TotalShipments,
        int ActiveShipments,
        int ShipmentsInCustoms,
        int DeliveredLast30Days,
        int TotalSuppliers,
        int TotalCustomers,
        int TotalWarehouses,
        int WarehouseUnloadingsLast30,
        decimal CashNetLast30,
        int CustomerInteractionsLast30)
    {
        public static DashboardSummary Empty { get; } = new(
            0, 0, 0, 0, 0, 0, 0, 0, 0m, 0);
    }
}
