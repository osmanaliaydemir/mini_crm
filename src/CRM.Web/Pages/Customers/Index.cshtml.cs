using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Domain.Customers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public IndexModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public const string UnspecifiedSegmentLabel = "__UNSPECIFIED__";

    public string? Search { get; set; }
    public IList<Customer> Customers { get; private set; } = new List<Customer>();
    public int TotalCustomers { get; private set; }
    public int DistinctSegmentCount { get; private set; }
    public int NewCustomersCount { get; private set; }
    public int RecentInteractionsCount { get; private set; }
    public string TopSegmentName { get; private set; } = UnspecifiedSegmentLabel;
    public int TopSegmentCustomerCount { get; private set; }
    public IReadOnlyList<CustomerSegmentStat> CustomerSegmentStats { get; private set; } = Array.Empty<CustomerSegmentStat>();
    public IReadOnlyList<TopCustomerStat> TopCustomerStats { get; private set; } = Array.Empty<TopCustomerStat>();
    public string MonthlyInteractionLabelsJson { get; private set; } = "[]";
    public string MonthlyInteractionDataJson { get; private set; } = "[]";

    public async Task OnGetAsync(string? search, CancellationToken cancellationToken)
    {
        Search = search;

        var customerQuery = _dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            customerQuery = customerQuery.Where(c =>
                EF.Functions.Like(c.Name, $"%{search}%") ||
                EF.Functions.Like(c.LegalName!, $"%{search}%") ||
                EF.Functions.Like(c.TaxNumber!, $"%{search}%"));
        }

        Customers = await customerQuery.OrderBy(c => c.Name).ToListAsync(cancellationToken);
        TotalCustomers = Customers.Count;

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var sixMonthsStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);

        NewCustomersCount = Customers.Count(c => c.CreatedAt >= thirtyDaysAgo);

        var segmentComparer = StringComparer.OrdinalIgnoreCase;
        var segmentStats = Customers
            .GroupBy(c => NormalizeSegment(c.Segment), segmentComparer)
            .Select(group => new CustomerSegmentStat(group.Key, group.Count()))
            .OrderByDescending(stat => stat.CustomerCount)
            .ThenBy(stat => stat.Segment, segmentComparer)
            .ToList();

        CustomerSegmentStats = segmentStats;
        DistinctSegmentCount = segmentStats.Count;

        var topSegment = segmentStats.FirstOrDefault();
        if (topSegment is not null)
        {
            TopSegmentName = topSegment.Segment;
            TopSegmentCustomerCount = topSegment.CustomerCount;
        }

        if (TotalCustomers == 0)
        {
            RecentInteractionsCount = 0;
            MonthlyInteractionLabelsJson = "[]";
            MonthlyInteractionDataJson = "[]";
            TopCustomerStats = Array.Empty<TopCustomerStat>();
            return;
        }

        var customerIdLookup = Customers.ToDictionary(c => c.Id);
        var customerIdSet = customerIdLookup.Keys.ToHashSet();

        var interactions = await _dbContext.CustomerInteractions
            .AsNoTracking()
            .Where(i => i.InteractionDate >= sixMonthsStart)
            .ToListAsync(cancellationToken);

        var relevantInteractions = interactions
            .Where(i => customerIdSet.Contains(i.CustomerId))
            .ToList();

        RecentInteractionsCount = relevantInteractions.Count(i => i.InteractionDate >= thirtyDaysAgo);

        var monthlyInteractionsRaw = relevantInteractions
            .GroupBy(i => new { i.InteractionDate.Year, i.InteractionDate.Month })
            .Select(g => new MonthlyInteractionDto(g.Key.Year, g.Key.Month, g.Count()))
            .ToList();

        var monthlyLookup = monthlyInteractionsRaw.ToDictionary(
            keySelector: item => (item.Year, item.Month),
            elementSelector: item => item.InteractionCount);

        var culture = CultureInfo.CurrentUICulture;
        var labels = new List<string>();
        var values = new List<int>();
        var cursor = new DateTime(sixMonthsStart.Year, sixMonthsStart.Month, 1);

        for (var i = 0; i < 6; i++)
        {
            var key = (cursor.Year, cursor.Month);
            monthlyLookup.TryGetValue(key, out var count);
            labels.Add($"{culture.DateTimeFormat.GetAbbreviatedMonthName(cursor.Month)} {cursor.Year}");
            values.Add(count);
            cursor = cursor.AddMonths(1);
        }

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        MonthlyInteractionLabelsJson = JsonSerializer.Serialize(labels, jsonOptions);
        MonthlyInteractionDataJson = JsonSerializer.Serialize(values, jsonOptions);

        var topCustomersRaw = relevantInteractions
            .Where(i => i.InteractionDate >= ninetyDaysAgo)
            .GroupBy(i => i.CustomerId)
            .Select(g =>
            {
                customerIdLookup.TryGetValue(g.Key, out var customer);
                var lastInteraction = g.Max(x => x.InteractionDate);
                return new TopCustomerDto(
                    g.Key,
                    customer?.Name,
                    customer?.Segment,
                    g.Count(),
                    lastInteraction);
            })
            .OrderByDescending(x => x.InteractionCount)
            .ThenByDescending(x => x.LastInteractionAt)
            .Take(5)
            .ToList();

        TopCustomerStats = topCustomersRaw
            .Select(dto => new TopCustomerStat(
                dto.CustomerId,
                dto.Name ?? "Customer",
                NormalizeSegment(dto.Segment),
                dto.InteractionCount,
                dto.LastInteractionAt))
            .ToList();
    }

    private static string NormalizeSegment(string? segment) =>
        string.IsNullOrWhiteSpace(segment) ? UnspecifiedSegmentLabel : segment.Trim();

    private sealed record MonthlyInteractionDto(int Year, int Month, int InteractionCount);

    private sealed record TopCustomerDto(Guid CustomerId, string? Name, string? Segment, int InteractionCount, DateTime LastInteractionAt);

    public sealed record CustomerSegmentStat(string Segment, int CustomerCount);

    public sealed record TopCustomerStat(Guid CustomerId, string Name, string Segment, int InteractionCount, DateTime LastInteractionAt);
}

