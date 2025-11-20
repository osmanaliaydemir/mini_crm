using CRM.Application.Common;
using CRM.Application.Search;
using CRM.Domain.Customers;
using CRM.Domain.Finance;
using CRM.Domain.Shipments;
using CRM.Domain.Suppliers;
using CRM.Domain.Tasks;
using CRM.Domain.Warehouses;
using CRM.Infrastructure.Identity;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Search;

public class GlobalSearchService : IGlobalSearchService
{
    private readonly IApplicationDbContext _context;
    private readonly CRMDbContext _dbContext;
    private readonly ILogger<GlobalSearchService> _logger;

    public GlobalSearchService(IApplicationDbContext context, CRMDbContext dbContext, ILogger<GlobalSearchService> logger)
    {
        _context = context;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GlobalSearchResponse> SearchAsync(GlobalSearchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
        {
            return new GlobalSearchResponse(
                Array.Empty<GlobalSearchResult>(),
                0,
                new Dictionary<SearchEntityType, int>());
        }

        var query = request.Query.Trim();
        var entityTypes = request.EntityTypes ?? Enum.GetValues<SearchEntityType>().ToList();
        var results = new List<GlobalSearchResult>();

        try
        {
            // Search Customers
            if (entityTypes.Contains(SearchEntityType.Customer))
            {
                var customers = await SearchCustomersAsync(query, cancellationToken);
                results.AddRange(customers);
            }

            // Search Shipments
            if (entityTypes.Contains(SearchEntityType.Shipment))
            {
                var shipments = await SearchShipmentsAsync(query, cancellationToken);
                results.AddRange(shipments);
            }

            // Search Suppliers
            if (entityTypes.Contains(SearchEntityType.Supplier))
            {
                var suppliers = await SearchSuppliersAsync(query, cancellationToken);
                results.AddRange(suppliers);
            }

            // Search Warehouses
            if (entityTypes.Contains(SearchEntityType.Warehouse))
            {
                var warehouses = await SearchWarehousesAsync(query, cancellationToken);
                results.AddRange(warehouses);
            }

            // Search Tasks
            if (entityTypes.Contains(SearchEntityType.Task))
            {
                var tasks = await SearchTasksAsync(query, cancellationToken);
                results.AddRange(tasks);
            }

            // Search Payment Plans
            if (entityTypes.Contains(SearchEntityType.PaymentPlan))
            {
                var paymentPlans = await SearchPaymentPlansAsync(query, cancellationToken);
                results.AddRange(paymentPlans);
            }

            // Search Cash Transactions
            if (entityTypes.Contains(SearchEntityType.CashTransaction))
            {
                var transactions = await SearchCashTransactionsAsync(query, cancellationToken);
                results.AddRange(transactions);
            }

            // Search Users
            if (entityTypes.Contains(SearchEntityType.User))
            {
                var users = await SearchUsersAsync(query, cancellationToken);
                results.AddRange(users);
            }

            // Search Customer Interactions
            if (entityTypes.Contains(SearchEntityType.CustomerInteraction))
            {
                var interactions = await SearchCustomerInteractionsAsync(query, cancellationToken);
                results.AddRange(interactions);
            }

            // Sort by relevance score and take max results
            var sortedResults = results.OrderByDescending(r => r.RelevanceScore)
                .ThenByDescending(r => r.CreatedAt).Take(request.MaxResults).ToList();

            // Count by type
            var countByType = results.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count());

            return new GlobalSearchResponse(
                sortedResults,
                sortedResults.Count,
                countByType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing global search for query: {Query}", query);
            return new GlobalSearchResponse(Array.Empty<GlobalSearchResult>(), 0, new Dictionary<SearchEntityType, int>());
        }
    }

    private async Task<List<GlobalSearchResult>> SearchCustomersAsync(string query, CancellationToken cancellationToken)
    {
        var customers = await _context.Customers.AsNoTracking()
            .Where(c => c.Name.Contains(query) ||
                       (c.LegalName != null && c.LegalName.Contains(query)) ||
                       (c.TaxNumber != null && c.TaxNumber.Contains(query)) ||
                       (c.Email != null && c.Email.Contains(query)) ||
                       (c.Phone != null && c.Phone.Contains(query)))
            .OrderBy(c => c.Name).Take(10).ToListAsync(cancellationToken);

        return customers.Select(c => new GlobalSearchResult(SearchEntityType.Customer,
            c.Id, c.Name, c.LegalName ?? c.Email, c.Segment,
            "customer", $"/Customers/Details/{c.Id}", c.CreatedAt, c.CreatedBy,
            CalculateRelevanceScore(c.Name, query, c.LegalName, c.TaxNumber, c.Email))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchShipmentsAsync(string query, CancellationToken cancellationToken)
    {
        var shipments = await _context.Shipments.AsNoTracking().Include(s => s.Customer)
            .Where(s => s.ReferenceNumber.Contains(query) ||
                       (s.Notes != null && s.Notes.Contains(query)) ||
                       (s.Customer != null && s.Customer.Name.Contains(query)))
            .OrderByDescending(s => s.CreatedAt).Take(10).ToListAsync(cancellationToken);

        return shipments.Select(s => new GlobalSearchResult(SearchEntityType.Shipment, s.Id, s.ReferenceNumber,
            s.Customer != null ? s.Customer.Name : null, s.Status.ToString(),
            "shipment", $"/Shipments/Details/{s.Id}", s.CreatedAt, s.CreatedBy,
            CalculateRelevanceScore(s.ReferenceNumber, query, s.Customer?.Name, s.Notes))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchSuppliersAsync(string query, CancellationToken cancellationToken)
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Name.Contains(query) ||
                       (s.Country != null && s.Country.Contains(query)) ||
                       (s.TaxNumber != null && s.TaxNumber.Contains(query)) ||
                       (s.ContactEmail != null && s.ContactEmail.Contains(query)))
            .OrderBy(s => s.Name)
            .Take(10)
            .ToListAsync(cancellationToken);

        return suppliers.Select(s => new GlobalSearchResult(SearchEntityType.Supplier, s.Id, s.Name,
            s.Country, s.ContactEmail, "supplier", $"/Suppliers/Details/{s.Id}", s.CreatedAt, s.CreatedBy,
            CalculateRelevanceScore(s.Name, query, s.Country, s.TaxNumber, s.ContactEmail))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchWarehousesAsync(string query, CancellationToken cancellationToken)
    {
        var warehouses = await _context.Warehouses.AsNoTracking()
            .Where(w => w.Name.Contains(query) || (w.Location != null && w.Location.Contains(query)))
            .OrderBy(w => w.Name).Take(10).ToListAsync(cancellationToken);

        return warehouses.Select(w => new GlobalSearchResult(SearchEntityType.Warehouse,
            w.Id, w.Name, w.Location, w.ContactPerson,
            "warehouse", $"/Warehouses/Details/{w.Id}", w.CreatedAt, w.CreatedBy,
            CalculateRelevanceScore(w.Name, query, w.Location))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchTasksAsync(string query, CancellationToken cancellationToken)
    {
        var tasks = await _context.Tasks.AsNoTracking()
            .Where(t => t.Title.Contains(query) || (t.Description != null && t.Description.Contains(query)))
            .OrderByDescending(t => t.CreatedAt).Take(10).ToListAsync(cancellationToken);

        return tasks.Select(t => new GlobalSearchResult(SearchEntityType.Task, t.Id, t.Title,
            t.Status.ToString(), t.Description, "task", $"/Tasks/Details/{t.Id}",
            t.CreatedAt, t.CreatedBy, CalculateRelevanceScore(t.Title, query, t.Description))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchPaymentPlansAsync(string query, CancellationToken cancellationToken)
    {
        // Get matching customer IDs
        var matchingCustomerIds = await _context.Customers.AsNoTracking().Where(c => c.Name.Contains(query))
            .Select(c => c.Id).ToListAsync(cancellationToken);

        // Get matching shipment IDs
        var matchingShipmentIds = await _context.Shipments.AsNoTracking().Where(s => s.ReferenceNumber.Contains(query))
            .Select(s => s.Id).ToListAsync(cancellationToken);

        var plans = await _context.PaymentPlans.AsNoTracking()
            .Where(p => matchingCustomerIds.Contains(p.CustomerId) ||
                       matchingShipmentIds.Contains(p.ShipmentId) ||
                       p.PlanType.ToString().Contains(query))
            .OrderByDescending(p => p.CreatedAt).Take(10).ToListAsync(cancellationToken);

        // Get customer and shipment names for display
        var customerIds = plans.Select(p => p.CustomerId).Distinct().ToList();
        var shipmentIds = plans.Select(p => p.ShipmentId).Distinct().ToList();

        var customers = await _context.Customers.AsNoTracking().Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var shipments = await _context.Shipments.AsNoTracking().Where(s => shipmentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.ReferenceNumber, cancellationToken);

        return plans.Select(p => new GlobalSearchResult(SearchEntityType.PaymentPlan,
            p.Id, $"Payment Plan - {p.PlanType}",
            customers.TryGetValue(p.CustomerId, out var customerName) ? customerName :
                (shipments.TryGetValue(p.ShipmentId, out var shipmentRef) ? shipmentRef : null),
            $"Amount: {p.TotalAmount:C}",
            "payment-plan",
            $"/Finance/PaymentPlans/Details/{p.Id}",
            p.CreatedAt,
            p.CreatedBy,
            CalculateRelevanceScore(p.PlanType.ToString(), query,
                customers.TryGetValue(p.CustomerId, out var cName) ? cName : null,
                shipments.TryGetValue(p.ShipmentId, out var sRef) ? sRef : null))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchCashTransactionsAsync(string query, CancellationToken cancellationToken)
    {
        var transactions = await _context.CashTransactions.AsNoTracking()
            .Where(t => (t.Description != null && t.Description.Contains(query)) ||
                       t.TransactionType.ToString().Contains(query))
            .OrderByDescending(t => t.TransactionDate).Take(10).ToListAsync(cancellationToken);

        return transactions.Select(t => new GlobalSearchResult(SearchEntityType.CashTransaction,
            t.Id, t.Description ?? "Cash Transaction",
            t.TransactionType.ToString(), $"Amount: {t.Amount:C}",
            "cash-transaction", $"/Finance/Cashbox", t.TransactionDate, null,
            CalculateRelevanceScore(t.Description ?? "", query, t.TransactionType.ToString()))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchUsersAsync(string query, CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users.AsNoTracking()
            .Where(u => (u.UserName != null && u.UserName.Contains(query)) ||
                       (u.Email != null && u.Email.Contains(query)))
            .OrderBy(u => u.UserName ?? u.Email ?? "").Take(10)
            .ToListAsync(cancellationToken);

        return users.Select(u => new GlobalSearchResult(
            SearchEntityType.User,
            u.Id,
            u.UserName ?? u.Email ?? "Unknown",
            u.Email,
            null,
            "user",
            $"/Users/Edit/{u.Id}",
            null,
            null,
            CalculateRelevanceScore(u.UserName ?? "", query, u.Email))).ToList();
    }

    private async Task<List<GlobalSearchResult>> SearchCustomerInteractionsAsync(string query, CancellationToken cancellationToken)
    {
        var interactions = await _context.CustomerInteractions
            .AsNoTracking()
            .Include(i => i.Customer)
            .Where(i => (i.Subject != null && i.Subject.Contains(query)) ||
                       (i.Notes != null && i.Notes.Contains(query)) ||
                       (i.Customer != null && i.Customer.Name.Contains(query)) ||
                       i.InteractionType.Contains(query))
            .OrderByDescending(i => i.InteractionDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        return interactions.Select(i => new GlobalSearchResult(
            SearchEntityType.CustomerInteraction,
            i.Id,
            i.Subject ?? i.InteractionType,
            i.Customer != null ? i.Customer.Name : null,
            i.Notes,
            "interaction",
            $"/Customers/Details/{i.CustomerId}",
            i.CreatedAt,
            i.CreatedBy,
            CalculateRelevanceScore(i.Subject ?? i.InteractionType, query, i.Customer?.Name, i.Notes))).ToList();
    }

    private static int CalculateRelevanceScore(string? primary, string query, params string?[] secondary)
    {
        if (string.IsNullOrWhiteSpace(primary))
        {
            return 0;
        }

        var score = 0;
        var queryLower = query.ToLowerInvariant();
        var primaryLower = primary.ToLowerInvariant();

        // Exact match gets highest score
        if (primaryLower == queryLower)
        {
            score += 100;
        }
        // Starts with query
        else if (primaryLower.StartsWith(queryLower))
        {
            score += 50;
        }
        // Contains query
        else if (primaryLower.Contains(queryLower))
        {
            score += 25;
        }

        // Check secondary fields
        foreach (var field in secondary)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                continue;
            }

            var fieldLower = field.ToLowerInvariant();
            if (fieldLower == queryLower)
            {
                score += 30;
            }
            else if (fieldLower.StartsWith(queryLower))
            {
                score += 15;
            }
            else if (fieldLower.Contains(queryLower))
            {
                score += 10;
            }
        }

        return score;
    }
}

