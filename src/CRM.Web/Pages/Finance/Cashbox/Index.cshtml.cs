using CRM.Domain.Customers;
using CRM.Domain.Finance;
using CRM.Domain.Shipments;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CRM.Web.Pages.Finance.Cashbox;

public class IndexModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public IndexModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
        TransactionTypeOptions = Enum.GetValues(typeof(CashTransactionType))
            .Cast<CashTransactionType>()
            .Select(type => new SelectListItem(type.ToString(), type.ToString()))
            .ToList();
    }

    public FilterModel Filter { get; private set; } = new();
    public IList<CashTransactionView> Transactions { get; private set; } = new List<CashTransactionView>();
    public CashSummary Summary { get; private set; } = new();
    public IList<SelectListItem> TransactionTypeOptions { get; }
    public string MonthlyLabelsJson { get; private set; } = "[]";
    public string MonthlyIncomeDataJson { get; private set; } = "[]";
    public string MonthlyExpenseDataJson { get; private set; } = "[]";
    public string MonthlyNetDataJson { get; private set; } = "[]";
    public IReadOnlyList<CategorySummary> CategorySummaries { get; private set; } = Array.Empty<CategorySummary>();

    public async Task OnGetAsync(DateTime? from, DateTime? to, string? type, CancellationToken cancellationToken)
    {
        Filter = new FilterModel
        {
            From = from,
            To = to,
            Type = type
        };

        var transactionEntities = await _dbContext.CashTransactions
            .AsNoTracking()
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        if (from.HasValue)
        {
            transactionEntities = transactionEntities
                .Where(t => t.TransactionDate >= from.Value)
                .ToList();
        }

        if (to.HasValue)
        {
            transactionEntities = transactionEntities
                .Where(t => t.TransactionDate <= to.Value)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<CashTransactionType>(type, out var parsedType))
        {
            transactionEntities = transactionEntities
                .Where(t => t.TransactionType == parsedType)
                .ToList();
        }

        var transactionResults = transactionEntities
            .Select(t => new CashTransactionView
            {
                Id = t.Id,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Currency = t.Currency,
                Description = t.Description,
                Category = t.Category,
                RelatedCustomerId = t.RelatedCustomerId,
                RelatedShipmentId = t.RelatedShipmentId
            })
            .ToList();

        var customerIds = transactionResults
            .Where(t => t.RelatedCustomerId.HasValue)
            .Select(t => t.RelatedCustomerId!.Value)
            .Distinct()
            .ToList();

        var shipmentIds = transactionResults
            .Where(t => t.RelatedShipmentId.HasValue)
            .Select(t => t.RelatedShipmentId!.Value)
            .Distinct()
            .ToList();

        var customerLookup = new Dictionary<Guid, string>();
        if (customerIds.Count > 0)
        {
            var customerPredicate = BuildCustomerPredicate(customerIds);
            var customerPairs = await _dbContext.Customers
                .AsNoTracking()
                .Where(customerPredicate)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(cancellationToken);

            customerLookup = customerPairs.ToDictionary(x => x.Id, x => x.Name);
        }

        var shipmentLookup = new Dictionary<Guid, string>();
        if (shipmentIds.Count > 0)
        {
            var shipmentPredicate = BuildShipmentPredicate(shipmentIds);
            var shipmentPairs = await _dbContext.Shipments
                .AsNoTracking()
                .Where(shipmentPredicate)
                .Select(s => new { s.Id, s.ReferenceNumber })
                .ToListAsync(cancellationToken);

            shipmentLookup = shipmentPairs.ToDictionary(x => x.Id, x => x.ReferenceNumber);
        }

        foreach (var transaction in transactionResults)
        {
            if (transaction.RelatedCustomerId.HasValue && customerLookup.TryGetValue(transaction.RelatedCustomerId.Value, out var customerName))
            {
                transaction.CustomerName = customerName;
            }

            if (transaction.RelatedShipmentId.HasValue && shipmentLookup.TryGetValue(transaction.RelatedShipmentId.Value, out var shipmentReference))
            {
                transaction.ShipmentReference = shipmentReference;
            }
        }

        Transactions = transactionResults;

        Summary = new CashSummary
        {
            TotalIncome = Transactions.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
            TotalExpense = Transactions.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount)
        };

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var monthlyStats = transactionResults
            .GroupBy(t => new DateTime(t.TransactionDate.Year, t.TransactionDate.Month, 1))
            .Select(group => new MonthlyAggregate(
                group.Key,
                group.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
                group.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount)))
            .OrderBy(stat => stat.Month)
            .ToList();

        const int monthsToShow = 6;
        if (monthlyStats.Count > monthsToShow)
        {
            monthlyStats = monthlyStats
                .Skip(Math.Max(0, monthlyStats.Count - monthsToShow))
                .ToList();
        }

        var culture = CultureInfo.CurrentUICulture;

        MonthlyLabelsJson = JsonSerializer.Serialize(
            monthlyStats.Select(stat => stat.Month.ToString("MMM yyyy", culture)),
            jsonOptions);

        MonthlyIncomeDataJson = JsonSerializer.Serialize(
            monthlyStats.Select(stat => stat.IncomeAmount),
            jsonOptions);

        MonthlyExpenseDataJson = JsonSerializer.Serialize(
            monthlyStats.Select(stat => stat.ExpenseAmount),
            jsonOptions);

        MonthlyNetDataJson = JsonSerializer.Serialize(
            monthlyStats.Select(stat => stat.NetAmount),
            jsonOptions);

        CategorySummaries = transactionResults
            .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? null : t.Category.Trim())
            .Select(group => new CategorySummary(
                group.Key,
                group.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
                group.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount)))
            .OrderByDescending(stat => stat.TotalMovement)
            .ThenBy(stat => stat.CategoryName ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
            .Take(5)
            .ToList();
    }

    private static Expression<Func<Customer, bool>> BuildCustomerPredicate(IReadOnlyList<Guid> ids)
    {
        var parameter = Expression.Parameter(typeof(Customer), "c");
        Expression? body = null;

        foreach (var id in ids)
        {
            var equalsExpression = Expression.Equal(
                Expression.Property(parameter, nameof(Customer.Id)),
                Expression.Constant(id));

            body = body is null ? equalsExpression : Expression.OrElse(body, equalsExpression);
        }

        body ??= Expression.Constant(false);
        return Expression.Lambda<Func<Customer, bool>>(body, parameter);
    }

    private static Expression<Func<Shipment, bool>> BuildShipmentPredicate(IReadOnlyList<Guid> ids)
    {
        var parameter = Expression.Parameter(typeof(Shipment), "s");
        Expression? body = null;

        foreach (var id in ids)
        {
            var equalsExpression = Expression.Equal(
                Expression.Property(parameter, nameof(Shipment.Id)),
                Expression.Constant(id));

            body = body is null ? equalsExpression : Expression.OrElse(body, equalsExpression);
        }

        body ??= Expression.Constant(false);
        return Expression.Lambda<Func<Shipment, bool>>(body, parameter);
    }

    public sealed class FilterModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Type { get; set; }
    }

    public sealed class CashTransactionView
    {
        public Guid Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public CashTransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public string? Description { get; set; }
        public string? Category { get; set; }
        public Guid? RelatedCustomerId { get; set; }
        public Guid? RelatedShipmentId { get; set; }
        public string? CustomerName { get; set; }
        public string? ShipmentReference { get; set; }
    }

    public sealed class CashSummary
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetBalance => TotalIncome - TotalExpense;
    }

    private sealed record MonthlyAggregate(DateTime Month, decimal IncomeAmount, decimal ExpenseAmount)
    {
        public decimal NetAmount => IncomeAmount - ExpenseAmount;
    }

    public sealed record CategorySummary(string? CategoryName, decimal IncomeAmount, decimal ExpenseAmount)
    {
        public decimal NetAmount => IncomeAmount - ExpenseAmount;
        public decimal TotalMovement => IncomeAmount + ExpenseAmount;
    }
}

