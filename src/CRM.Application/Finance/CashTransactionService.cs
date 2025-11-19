using CRM.Application.Common;
using CRM.Application.Common.Caching;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Domain.Customers;
using CRM.Domain.Finance;
using CRM.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Finance;

public class CashTransactionService : ICashTransactionService
{
    private readonly IRepository<CashTransaction> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CashTransactionService> _logger;

    public CashTransactionService(
        IRepository<CashTransaction> repository, 
        IApplicationDbContext context, 
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<CashTransactionService> logger)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateCashTransactionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating cash transaction: Type: {TransactionType}, Amount: {Amount}, Currency: {Currency}, Date: {TransactionDate}", 
                request.TransactionType, request.Amount, request.Currency, request.TransactionDate);

            var transaction = new CashTransaction(Guid.NewGuid(), request.TransactionDate, request.TransactionType, request.Amount,
                request.Currency, request.Description, request.Category, request.RelatedCustomerId, request.RelatedShipmentId);

            await _repository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cash transaction created successfully: {TransactionId}, Type: {TransactionType}, Amount: {Amount}", 
                transaction.Id, transaction.TransactionType, transaction.Amount);

            // Cache invalidation - Cash transaction ve ana dashboard cache'lerini temizle
            // Cache işlemleri başarısız olsa bile devam et
            try
            {
                await _cacheService.RemoveByPrefixAsync(CacheKeys.CashTransactionDashboardPrefix, cancellationToken);
                await _cacheService.RemoveAsync(CacheKeys.CashboxQuickSummary, cancellationToken);
                await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed for cash transaction creation");
            }

            return transaction.Id;
        }
        catch (Exception ex) when (ex is not NotFoundException && ex is not BadRequestException && ex is not ValidationException)
        {
            _logger.LogError(ex, "Error creating cash transaction: {TransactionType}, Amount: {Amount}", request.TransactionType, request.Amount);
            throw;
        }
    }

    public async Task<IReadOnlyList<CashTransactionDto>> GetAllAsync(DateTime? from = null, DateTime? to = null, CashTransactionType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.CashTransactions.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= to.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync(cancellationToken);

        // Get related customer and shipment names
        var customerIds = transactions.Where(t => t.RelatedCustomerId.HasValue).Select(t => t.RelatedCustomerId!.Value).Distinct().ToHashSet();

        var shipmentIds = transactions.Where(t => t.RelatedShipmentId.HasValue).Select(t => t.RelatedShipmentId!.Value).Distinct().ToHashSet();

        // Fetch all relevant customers and shipments to avoid SQL parameter limits
        // Filter in memory instead of using Contains() in SQL query
        Dictionary<Guid, string> customers;
        if (customerIds.Count > 0)
        {
            var allCustomers = await _context.Customers.AsNoTracking().Select(c => new { c.Id, c.Name }).ToListAsync(cancellationToken);

            customers = allCustomers.Where(c => customerIds.Contains(c.Id)).ToDictionary(c => c.Id, c => c.Name);
        }
        else
        {
            customers = new Dictionary<Guid, string>();
        }

        Dictionary<Guid, string> shipments;
        if (shipmentIds.Count > 0)
        {
            var allShipments = await _context.Shipments.AsNoTracking().Select(s => new { s.Id, s.ReferenceNumber }).ToListAsync(cancellationToken);

            shipments = allShipments.Where(s => shipmentIds.Contains(s.Id)).ToDictionary(s => s.Id, s => s.ReferenceNumber);
        }
        else
        {
            shipments = new Dictionary<Guid, string>();
        }

        return transactions.Select(t => new CashTransactionDto(t.Id, t.TransactionDate, t.TransactionType,
            t.Amount, t.Currency, t.Description, t.Category, t.RelatedCustomerId,
            t.RelatedCustomerId.HasValue && customers.TryGetValue(t.RelatedCustomerId.Value, out var customerName) ? customerName : null,
            t.RelatedShipmentId,
            t.RelatedShipmentId.HasValue && shipments.TryGetValue(t.RelatedShipmentId.Value, out var shipmentRef) ? shipmentRef : null,
            t.CreatedAt, t.RowVersion)).ToList();
    }

    public async Task<PagedResult<CashTransactionDto>> GetAllPagedAsync(PaginationRequest pagination, DateTime? from = null, DateTime? to = null, CashTransactionType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.CashTransactions.AsNoTracking().AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= to.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        var pagedResult = await query.OrderByDescending(t => t.TransactionDate)
            .ToPagedResultAsync(pagination, cancellationToken);

        // Get related customer and shipment names
        var customerIds = pagedResult.Items.Where(t => t.RelatedCustomerId.HasValue).Select(t => t.RelatedCustomerId!.Value).Distinct().ToHashSet();
        var shipmentIds = pagedResult.Items.Where(t => t.RelatedShipmentId.HasValue).Select(t => t.RelatedShipmentId!.Value).Distinct().ToHashSet();

        // Fetch all relevant customers and shipments
        Dictionary<Guid, string> customers;
        if (customerIds.Count > 0)
        {
            var allCustomers = await _context.Customers.AsNoTracking().Select(c => new { c.Id, c.Name }).ToListAsync(cancellationToken);
            customers = allCustomers.Where(c => customerIds.Contains(c.Id)).ToDictionary(c => c.Id, c => c.Name);
        }
        else
        {
            customers = new Dictionary<Guid, string>();
        }

        Dictionary<Guid, string> shipments;
        if (shipmentIds.Count > 0)
        {
            var allShipments = await _context.Shipments.AsNoTracking().Select(s => new { s.Id, s.ReferenceNumber }).ToListAsync(cancellationToken);
            shipments = allShipments.Where(s => shipmentIds.Contains(s.Id)).ToDictionary(s => s.Id, s => s.ReferenceNumber);
        }
        else
        {
            shipments = new Dictionary<Guid, string>();
        }

        var items = pagedResult.Items.Select(t => new CashTransactionDto(t.Id, t.TransactionDate, t.TransactionType,
            t.Amount, t.Currency, t.Description, t.Category, t.RelatedCustomerId,
            t.RelatedCustomerId.HasValue && customers.TryGetValue(t.RelatedCustomerId.Value, out var customerName) ? customerName : null,
            t.RelatedShipmentId,
            t.RelatedShipmentId.HasValue && shipments.TryGetValue(t.RelatedShipmentId.Value, out var shipmentRef) ? shipmentRef : null,
            t.CreatedAt, t.RowVersion)).ToList();

        return new PagedResult<CashTransactionDto>(items, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<CashTransactionDashboardData> GetDashboardDataAsync(DateTime? from = null, DateTime? to = null, CashTransactionType? type = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.CashTransactionDashboard(from, to, type?.ToString());
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await LoadCashTransactionDashboardDataAsync(from, to, type, cancellationToken),
            TimeSpan.FromMinutes(3), // Cashbox için daha kısa cache (finansal veri)
            cancellationToken);
    }

    private async Task<CashTransactionDashboardData> LoadCashTransactionDashboardDataAsync(DateTime? from, DateTime? to, CashTransactionType? type, CancellationToken cancellationToken)
    {
        var transactions = await GetAllAsync(from, to, type, cancellationToken);

        var summary = new CashSummary(
            transactions.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
            transactions.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount));

        var monthlyStats = transactions
            .GroupBy(t => new DateTime(t.TransactionDate.Year, t.TransactionDate.Month, 1))
            .Select(group => new
            {
                Month = group.Key,
                IncomeAmount = group.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
                ExpenseAmount = group.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(stat => stat.Month)
            .ToList();

        const int monthsToShow = 6;
        var monthlyStatsToShow = monthlyStats.Count > monthsToShow
            ? monthlyStats.Skip(Math.Max(0, monthlyStats.Count - monthsToShow)).ToList()
            : monthlyStats;

        var culture = System.Globalization.CultureInfo.CurrentUICulture;

        var monthlyLabels = monthlyStatsToShow.Select(stat => stat.Month.ToString("MMM yyyy", culture)).ToList();

        var monthlyIncomeData = monthlyStatsToShow.Select(stat => stat.IncomeAmount).ToList();

        var monthlyExpenseData = monthlyStatsToShow.Select(stat => stat.ExpenseAmount).ToList();

        var monthlyNetData = monthlyStatsToShow.Select(stat => stat.IncomeAmount - stat.ExpenseAmount).ToList();

        var categorySummaries = transactions.GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? null : t.Category.Trim())
            .Select(group => new CategorySummary(
                group.Key,
                group.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
                group.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount)))
            .OrderByDescending(stat => stat.TotalMovement)
            .ThenBy(stat => stat.CategoryName ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
            .Take(5)
            .ToList();

        // En son 3 hareketi al (tarihe göre sıralı)
        var recentTransactions = transactions
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Take(3)
            .ToList();

        return new CashTransactionDashboardData(transactions, summary, categorySummaries, recentTransactions,
            monthlyLabels, monthlyIncomeData, monthlyExpenseData, monthlyNetData);
    }
}

