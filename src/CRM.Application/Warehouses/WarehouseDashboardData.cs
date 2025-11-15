namespace CRM.Application.Warehouses;

public record WarehouseDashboardData(
    IReadOnlyList<WarehouseDto> Warehouses,
    int TotalWarehouses,
    int DistinctLocationCount,
    int ActiveWarehousesCount,
    int RecentUnloadingCount,
    string TopLocationName,
    int TopLocationWarehouseCount,
    IReadOnlyList<WarehouseLocationStat> WarehouseLocationStats,
    IReadOnlyList<TopWarehouseStat> TopWarehouseStats,
    IReadOnlyList<string> MonthlyVolumeLabels,
    IReadOnlyList<decimal> MonthlyVolumeData);

public record WarehouseLocationStat(string Location, int WarehouseCount);

public record TopWarehouseStat(
    Guid WarehouseId,
    string Name,
    string Location,
    decimal TotalVolume,
    int TripCount,
    DateTime LastUnloadedAt);

