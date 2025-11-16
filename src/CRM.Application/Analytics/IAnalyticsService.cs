namespace CRM.Application.Analytics;

public interface IAnalyticsService
{
    Task<AnalyticsData> GetOperationsAnalyticsAsync(CancellationToken cancellationToken = default);
}

