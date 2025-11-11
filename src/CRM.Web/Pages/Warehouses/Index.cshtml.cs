using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Domain.Warehouses;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Warehouses;

public class IndexModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public IndexModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public const string UnspecifiedLocationLabel = "__UNSPECIFIED__";

    public IList<Warehouse> Warehouses { get; private set; } = new List<Warehouse>();
    public int TotalWarehouses { get; private set; }
    public int DistinctLocationCount { get; private set; }
    public int ActiveWarehousesCount { get; private set; }
    public int RecentUnloadingCount { get; private set; }
    public string TopLocationName { get; private set; } = UnspecifiedLocationLabel;
    public int TopLocationWarehouseCount { get; private set; }
    public IReadOnlyList<WarehouseLocationStat> WarehouseLocationStats { get; private set; } = Array.Empty<WarehouseLocationStat>();
    public IReadOnlyList<TopWarehouseStat> TopWarehouseStats { get; private set; } = Array.Empty<TopWarehouseStat>();
    public string MonthlyVolumeLabelsJson { get; private set; } = "[]";
    public string MonthlyVolumeDataJson { get; private set; } = "[]";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var warehouses = await _dbContext.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        Warehouses = warehouses;
        TotalWarehouses = Warehouses.Count;

        var locationComparer = StringComparer.OrdinalIgnoreCase;
        var locationStats = Warehouses
            .GroupBy(w => NormalizeLocation(w.Location), locationComparer)
            .Select(group => new WarehouseLocationStat(group.Key, group.Count()))
            .OrderByDescending(stat => stat.WarehouseCount)
            .ThenBy(stat => stat.Location, locationComparer)
            .ToList();

        WarehouseLocationStats = locationStats;
        DistinctLocationCount = locationStats.Count;

        var topLocation = locationStats.FirstOrDefault();
        if (topLocation is not null)
        {
            TopLocationName = topLocation.Location;
            TopLocationWarehouseCount = topLocation.WarehouseCount;
        }

        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var activeThreshold = now.AddDays(-60);
        var sixMonthsStart = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var periodStart = now.AddDays(-180);

        RecentUnloadingCount = await _dbContext.WarehouseUnloadings
            .AsNoTracking()
            .CountAsync(u => u.UnloadedAt >= thirtyDaysAgo, cancellationToken);

        var activeWarehouseIds = await _dbContext.WarehouseUnloadings
            .AsNoTracking()
            .Where(u => u.UnloadedAt >= activeThreshold)
            .Select(u => u.WarehouseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        ActiveWarehousesCount = activeWarehouseIds.Count;

        var monthlyVolumesRaw = await _dbContext.WarehouseUnloadings
            .AsNoTracking()
            .Where(u => u.UnloadedAt >= sixMonthsStart)
            .GroupBy(u => new { u.UnloadedAt.Year, u.UnloadedAt.Month })
            .Select(g => new MonthlyVolumeDto(g.Key.Year, g.Key.Month, g.Sum(x => x.UnloadedVolume)))
            .ToListAsync(cancellationToken);

        var monthlyLookup = monthlyVolumesRaw.ToDictionary(
            keySelector: item => (item.Year, item.Month),
            elementSelector: item => item.TotalVolume);

        var labels = new List<string>();
        var values = new List<decimal>();
        var culture = CultureInfo.CurrentUICulture;
        var cursor = new DateTime(sixMonthsStart.Year, sixMonthsStart.Month, 1);

        for (var i = 0; i < 6; i++)
        {
            var key = (cursor.Year, cursor.Month);
            monthlyLookup.TryGetValue(key, out var volume);
            labels.Add($"{culture.DateTimeFormat.GetAbbreviatedMonthName(cursor.Month)} {cursor.Year}");
            values.Add(Math.Round(volume, 2));
            cursor = cursor.AddMonths(1);
        }

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        MonthlyVolumeLabelsJson = JsonSerializer.Serialize(labels, jsonOptions);
        MonthlyVolumeDataJson = JsonSerializer.Serialize(values, jsonOptions);

        var topWarehouseRaw = await _dbContext.WarehouseUnloadings
            .AsNoTracking()
            .Where(u => u.UnloadedAt >= periodStart)
            .GroupBy(u => u.WarehouseId)
            .Select(g => new
            {
                WarehouseId = g.Key,
                TotalVolume = g.Sum(x => x.UnloadedVolume),
                TripCount = g.Count(),
                LastUnloadedAt = g.Max(x => x.UnloadedAt)
            })
            .OrderByDescending(x => x.TotalVolume)
            .ThenByDescending(x => x.LastUnloadedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var warehouseLookup = Warehouses.ToDictionary(w => w.Id);

        TopWarehouseStats = topWarehouseRaw
            .Select(item =>
            {
                warehouseLookup.TryGetValue(item.WarehouseId, out var warehouse);
                var name = warehouse?.Name ?? "Warehouse";
                var location = NormalizeLocation(warehouse?.Location);

                return new TopWarehouseStat(
                    item.WarehouseId,
                    name,
                    location,
                    Math.Round(item.TotalVolume, 2),
                    item.TripCount,
                    item.LastUnloadedAt);
            })
            .ToList();
    }

    private static string NormalizeLocation(string? location) =>
        string.IsNullOrWhiteSpace(location) ? UnspecifiedLocationLabel : location.Trim();

    private sealed record MonthlyVolumeDto(int Year, int Month, decimal TotalVolume);

    public sealed record WarehouseLocationStat(string Location, int WarehouseCount);

    public sealed record TopWarehouseStat(Guid WarehouseId, string Name, string Location, decimal TotalVolume, int TripCount, DateTime LastUnloadedAt);
}

