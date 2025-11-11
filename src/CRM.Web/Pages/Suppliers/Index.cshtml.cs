using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Domain.Suppliers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Suppliers;

public class IndexModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public IndexModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IList<Supplier> Suppliers { get; private set; } = new List<Supplier>();
    public int TotalSuppliers { get; private set; }
    public int DistinctCountryCount { get; private set; }
    public int RecentSuppliersCount { get; private set; }
    public string TopCountryName { get; private set; } = "-";
    public int TopCountrySupplierCount { get; private set; }
    public IReadOnlyList<SupplierCountryStat> SupplierCountryStats { get; private set; } = Array.Empty<SupplierCountryStat>();
    public string SupplierCountryChartLabelsJson { get; private set; } = "[]";
    public string SupplierCountryChartDataJson { get; private set; } = "[]";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var suppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        Suppliers = suppliers;
        TotalSuppliers = Suppliers.Count;

        var countryComparer = StringComparer.OrdinalIgnoreCase;
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        SupplierCountryStats = Suppliers
            .GroupBy(s => NormalizeCountry(s.Country), countryComparer)
            .Select(group => new SupplierCountryStat(group.Key, group.Count()))
            .OrderByDescending(stat => stat.SupplierCount)
            .ThenBy(stat => stat.Country, countryComparer)
            .ToList();

        DistinctCountryCount = SupplierCountryStats.Count;
        RecentSuppliersCount = Suppliers.Count(s => s.CreatedAt >= thirtyDaysAgo);

        var topCountry = SupplierCountryStats.FirstOrDefault();
        if (topCountry is not null)
        {
            TopCountryName = topCountry.Country;
            TopCountrySupplierCount = topCountry.SupplierCount;
        }

        var labels = SupplierCountryStats.Select(stat => stat.Country).ToArray();
        var values = SupplierCountryStats.Select(stat => stat.SupplierCount).ToArray();

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        SupplierCountryChartLabelsJson = JsonSerializer.Serialize(labels, jsonOptions);
        SupplierCountryChartDataJson = JsonSerializer.Serialize(values, jsonOptions);
    }

    private static string NormalizeCountry(string? country) =>
        string.IsNullOrWhiteSpace(country) ? "Belirtilmedi" : country.Trim();

    public sealed record SupplierCountryStat(string Country, int SupplierCount);
}

