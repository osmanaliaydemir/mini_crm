namespace CRM.Application.Customers;

public record CustomerDashboardData(
    IReadOnlyList<CustomerListItemDto> Customers,
    int TotalCustomers,
    int NewCustomersCount,
    int RecentInteractionsCount,
    int DistinctSegmentCount,
    string TopSegmentName,
    int TopSegmentCustomerCount,
    IReadOnlyList<CustomerSegmentStat> CustomerSegmentStats,
    IReadOnlyList<TopCustomerStat> TopCustomerStats,
    IReadOnlyList<string> MonthlyInteractionLabels,
    IReadOnlyList<int> MonthlyInteractionData);

public record CustomerSegmentStat(string Segment, int CustomerCount);

public record TopCustomerStat(Guid CustomerId, string Name, string Segment, int InteractionCount, DateTime LastInteractionAt);

public record AddInteractionRequest(
    DateTime InteractionDate,
    string InteractionType,
    string? Subject,
    string? Notes,
    string? RecordedBy);

