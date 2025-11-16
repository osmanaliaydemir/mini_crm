namespace CRM.Application.Common.Caching;

public static class CacheKeys
{
    public const string DashboardData = "dashboard:data";
    public const string AnalyticsOperations = "analytics:operations";
    public const string CustomerDashboardPrefix = "dashboard:customers";
    public const string SupplierDashboard = "dashboard:suppliers";
    public const string WarehouseDashboard = "dashboard:warehouses";
    public const string ShipmentDashboard = "dashboard:shipments";
    public const string CashTransactionDashboardPrefix = "dashboard:cash";
    public const string CashboxQuickSummary = "cashbox:quick:summary";
    
    public static string CustomerDashboard(string? search) => 
        string.IsNullOrWhiteSpace(search) 
            ? $"{CustomerDashboardPrefix}" 
            : $"{CustomerDashboardPrefix}:search:{search.ToLowerInvariant()}";
    
    public static string CashTransactionDashboard(DateTime? from, DateTime? to, string? type) =>
        $"{CashTransactionDashboardPrefix}:from:{from?.ToString("yyyy-MM-dd") ?? "all"}:to:{to?.ToString("yyyy-MM-dd") ?? "all"}:type:{type ?? "all"}";
}

