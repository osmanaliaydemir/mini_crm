namespace CRM.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}

