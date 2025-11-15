namespace CRM.Application.Suppliers;

public record SupplierDashboardData(
    IReadOnlyList<SupplierDto> Suppliers,
    int TotalSuppliers,
    int RecentSuppliersCount,
    int DistinctCountryCount,
    string TopCountryName,
    int TopCountrySupplierCount,
    IReadOnlyList<SupplierCountryStat> SupplierCountryStats,
    IReadOnlyList<string> CountryChartLabels,
    IReadOnlyList<int> CountryChartData);

public record SupplierCountryStat(string Country, int SupplierCount);

