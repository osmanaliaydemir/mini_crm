using CRM.Domain.Tasks;

namespace CRM.Application.Dashboard;

public sealed record DashboardData
{
    public DashboardSummary Summary { get; init; } = DashboardSummary.Empty;
    public IReadOnlyList<StatusSummary> ShipmentStatusSummaries { get; init; } = Array.Empty<StatusSummary>();
    public IReadOnlyList<TimeSeriesPoint> ShipmentMonthlyTrend { get; init; } = Array.Empty<TimeSeriesPoint>();
    public IReadOnlyList<CategoryPoint> SupplierCountryBreakdown { get; init; } = Array.Empty<CategoryPoint>();
    public IReadOnlyList<TimeSeriesPoint> WarehouseVolumeTrend { get; init; } = Array.Empty<TimeSeriesPoint>();
    public IReadOnlyList<CashFlowPoint> CashFlowTrend { get; init; } = Array.Empty<CashFlowPoint>();
    public IReadOnlyList<TimeSeriesPoint> CustomerInteractionTrend { get; init; } = Array.Empty<TimeSeriesPoint>();
    public Dictionary<string, IReadOnlyList<ActivityEvent>> ActivityFeed { get; init; } = new();
    public IReadOnlyList<TaskSummary> TodayTasks { get; init; } = Array.Empty<TaskSummary>();
}

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
    public static DashboardSummary Empty { get; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0m, 0);
}

public sealed record StatusSummary(Domain.Enums.ShipmentStatus Status, int Count);

public sealed record TimeSeriesPoint(DateTime PeriodStart, decimal Value);

public sealed record CashFlowPoint(DateTime PeriodStart, decimal Income, decimal Expense);

public sealed record CategoryPoint(string Label, int Value);

public sealed record ActivityEvent(DateTime OccurredAt, Domain.Enums.ShipmentStatus Status, string ReferenceNumber, string? CustomerDisplay);

public sealed record TaskSummary(Guid Id, string Title, Domain.Tasks.TaskStatus Status, Domain.Tasks.TaskPriority Priority, DateTime? DueDate, Guid? AssignedToUserId, string? AssignedToUserName);

