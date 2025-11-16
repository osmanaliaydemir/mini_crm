using CRM.Domain.Enums;

namespace CRM.Application.Analytics;

public sealed record AnalyticsData
{
    public OperationsSummary Summary { get; init; } = OperationsSummary.Empty;
    public IReadOnlyList<StatusBreakdownItem> StatusBreakdown { get; init; } = Array.Empty<StatusBreakdownItem>();
    public IReadOnlyList<CompletionTrendPoint> CompletionTrend { get; init; } = Array.Empty<CompletionTrendPoint>();
    public IReadOnlyList<WarehouseThroughputItem> WarehouseThroughput { get; init; } = Array.Empty<WarehouseThroughputItem>();
    public IReadOnlyList<DelayShipmentItem> DelayShipments { get; init; } = Array.Empty<DelayShipmentItem>();
}

public sealed record OperationsSummary(int TotalShipments, int ActiveShipments, double AvgTransitDays, double CustomsBacklogDays)
{
    public static OperationsSummary Empty { get; } = new(0, 0, 0, 0);
}

public sealed record StatusBreakdownItem(ShipmentStatus Status, int Count);

public sealed record CompletionTrendPoint(DateTime PeriodStart, int CompletedCount);

public sealed record WarehouseThroughputItem(string WarehouseName, int UnloadingCount, decimal TotalVolume);

public sealed record DelayShipmentItem(string ReferenceNumber, string? CustomerDisplay, ShipmentStatus Status, double DaysInStage);

