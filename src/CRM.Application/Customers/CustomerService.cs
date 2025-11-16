using CRM.Application.Common;
using CRM.Application.Common.Caching;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Domain.Customers;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IRepository<Customer> repository, 
        IApplicationDbContext context, 
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<CustomerService> logger)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.AsNoTracking().Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
        {
            return null;
        }

        var primaryContact = customer.Contacts.FirstOrDefault();

        return new CustomerDto(customer.Id, customer.Name, customer.LegalName, customer.TaxNumber,
            customer.Email, customer.Phone, customer.Address, customer.Segment, customer.Notes,
            primaryContact?.FullName, primaryContact?.Email, primaryContact?.Phone, primaryContact?.Position,
            customer.CreatedAt, customer.CreatedBy, customer.LastModifiedAt, customer.LastModifiedBy, customer.RowVersion);
    }

    public async Task<IReadOnlyList<CustomerListItemDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) || (c.LegalName != null && c.LegalName.Contains(search)) ||
                (c.TaxNumber != null && c.TaxNumber.Contains(search)));
        }

        var customers = await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return customers.Select(c => new CustomerListItemDto(c.Id, c.Name,
            c.LegalName, c.Segment, c.Email, c.Phone, c.Notes)).ToList();
    }

    public async Task<PagedResult<CustomerListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) || (c.LegalName != null && c.LegalName.Contains(search)) ||
                (c.TaxNumber != null && c.TaxNumber.Contains(search)));
        }

        var pagedResult = await query
            .OrderBy(c => c.Name)
            .ToPagedResultAsync(pagination, cancellationToken);

        var items = pagedResult.Items.Select(c => new CustomerListItemDto(c.Id, c.Name,
            c.LegalName, c.Segment, c.Email, c.Phone, c.Notes)).ToList();

        return new PagedResult<CustomerListItemDto>(items, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<Guid> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer(Guid.NewGuid(), request.Name, request.LegalName,
            request.TaxNumber, request.Email, request.Phone, request.Address, request.Segment, request.Notes);

        if (!string.IsNullOrWhiteSpace(request.PrimaryContactName))
        {
            customer.AddContact(request.PrimaryContactName, request.PrimaryContactEmail,
                request.PrimaryContactPhone, request.PrimaryContactPosition);
        }

        await _repository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation - Customer dashboard cache'lerini temizle
        await _cacheService.RemoveByPrefixAsync(CacheKeys.CustomerDashboardPrefix, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);

        return customer.Id;
    }

    public async Task UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.Id);
        }

        // Set RowVersion for optimistic concurrency control
        customer.RowVersion = request.RowVersion;

        customer.Update(request.Name, request.LegalName, request.TaxNumber, request.Email,
            request.Phone, request.Address, request.Segment, request.Notes);

        customer.ClearContacts();
        if (!string.IsNullOrWhiteSpace(request.PrimaryContactName))
        {
            customer.AddContact(request.PrimaryContactName, request.PrimaryContactEmail,
                request.PrimaryContactPhone, request.PrimaryContactPosition);
        }

        await _repository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation - Customer dashboard cache'lerini temizle
        await _cacheService.RemoveByPrefixAsync(CacheKeys.CustomerDashboardPrefix, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByIdAsync(id, cancellationToken);
        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), id);
        }

        await _repository.DeleteAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation - Customer dashboard cache'lerini temizle
        await _cacheService.RemoveByPrefixAsync(CacheKeys.CustomerDashboardPrefix, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);
    }

    public async Task<CustomerDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.AsNoTracking().Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
        {
            return null;
        }

        var primaryContact = customer.Contacts.FirstOrDefault();
        var customerDto = new CustomerDto(customer.Id, customer.Name, customer.LegalName, customer.TaxNumber,
            customer.Email, customer.Phone, customer.Address, customer.Segment, customer.Notes,
            primaryContact?.FullName, primaryContact?.Email, primaryContact?.Phone, primaryContact?.Position,
            customer.CreatedAt, customer.CreatedBy, customer.LastModifiedAt, customer.LastModifiedBy, customer.RowVersion);

        var contacts = await _context.CustomerContacts.AsNoTracking()
            .Where(contact => contact.CustomerId == id).OrderBy(contact => contact.FullName).ToListAsync(cancellationToken);

        var contactDtos = contacts.Select(c => new CustomerContactDto(c.Id, c.CustomerId, c.FullName,
            c.Email, c.Phone, c.Position)).ToList();

        var interactions = await _context.CustomerInteractions.AsNoTracking()
            .Where(i => i.CustomerId == id).OrderByDescending(i => i.InteractionDate).ToListAsync(cancellationToken);

        var interactionDtos = interactions.Select(i => new CustomerInteractionDto(i.Id, i.CustomerId, i.InteractionDate,
            i.InteractionType, i.Subject, i.Notes, i.RecordedBy, i.CreatedAt)).ToList();

        return new CustomerDetailsDto(customerDto, contactDtos, interactionDtos);
    }

    public async Task<Guid> AddInteractionAsync(Guid customerId, AddInteractionRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _repository.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), customerId);
        }

        var interaction = new CustomerInteraction(customerId, request.InteractionDate, request.InteractionType,
            request.Subject, request.Notes, request.RecordedBy);

        _context.CustomerInteractions.Add(interaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation - Customer dashboard ve ana dashboard cache'lerini temizle
        await _cacheService.RemoveByPrefixAsync(CacheKeys.CustomerDashboardPrefix, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);

        return interaction.Id;
    }

    public async Task<CustomerDashboardData> GetDashboardDataAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.CustomerDashboard(search);
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () => await LoadCustomerDashboardDataAsync(search, cancellationToken),
            TimeSpan.FromMinutes(5),
            cancellationToken);
    }

    private async Task<CustomerDashboardData> LoadCustomerDashboardDataAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.Name.Contains(search) ||
                (c.LegalName != null && c.LegalName.Contains(search)) ||
                (c.TaxNumber != null && c.TaxNumber.Contains(search)));
        }

        var customers = await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);

        var customerListItems = customers.Select(c => new CustomerListItemDto(c.Id, c.Name, c.LegalName,
            c.Segment, c.Email, c.Phone, c.Notes)).ToList();

        var totalCustomers = customers.Count;

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var sixMonthsStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);

        var newCustomersCount = customers.Count(c => c.CreatedAt >= thirtyDaysAgo);

        const string unspecifiedSegmentLabel = "__UNSPECIFIED__";
        var segmentComparer = StringComparer.OrdinalIgnoreCase;

        var segmentStats = customers.GroupBy(c => NormalizeSegment(c.Segment, unspecifiedSegmentLabel), segmentComparer)
            .Select(group => new CustomerSegmentStat(group.Key, group.Count())).OrderByDescending(stat => stat.CustomerCount)
            .ThenBy(stat => stat.Segment, segmentComparer).ToList();

        var distinctSegmentCount = segmentStats.Count;
        var topSegment = segmentStats.FirstOrDefault();
        var topSegmentName = topSegment?.Segment ?? unspecifiedSegmentLabel;
        var topSegmentCustomerCount = topSegment?.CustomerCount ?? 0;

        if (totalCustomers == 0)
        {
            return new CustomerDashboardData(customerListItems, totalCustomers, newCustomersCount,
                0, distinctSegmentCount, topSegmentName, topSegmentCustomerCount, segmentStats,
                Array.Empty<TopCustomerStat>(), Array.Empty<string>(), Array.Empty<int>());
        }

        var customerIds = customers.Select(c => c.Id).ToHashSet();
        var customerIdLookup = customers.ToDictionary(c => c.Id);

        // Tüm interaction'ları çekip sonra memory'de filtrele
        // Bu, SQL Server'ın parametre limiti sorununu önler
        var allInteractions = await _context.CustomerInteractions.AsNoTracking()
            .Where(i => i.InteractionDate >= sixMonthsStart).ToListAsync(cancellationToken);

        var interactions = allInteractions.Where(i => customerIds.Contains(i.CustomerId)).ToList();

        var recentInteractionsCount = interactions.Count(i => i.InteractionDate >= thirtyDaysAgo);

        var monthlyInteractionsRaw = interactions.GroupBy(i => new { i.InteractionDate.Year, i.InteractionDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() }).ToList();

        var monthlyLookup = monthlyInteractionsRaw.ToDictionary(keySelector: item => (item.Year, item.Month),
            elementSelector: item => item.Count);

        var culture = System.Globalization.CultureInfo.CurrentUICulture;
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

        var topCustomersRaw = interactions.Where(i => i.InteractionDate >= ninetyDaysAgo).GroupBy(i => i.CustomerId)
            .Select(g =>
            {
                customerIdLookup.TryGetValue(g.Key, out var customer);
                var lastInteraction = g.Max(x => x.InteractionDate);
                return new
                {
                    CustomerId = g.Key,
                    Name = customer?.Name,
                    Segment = customer?.Segment,
                    InteractionCount = g.Count(),
                    LastInteractionAt = lastInteraction
                };
            }).OrderByDescending(x => x.InteractionCount).ThenByDescending(x => x.LastInteractionAt).Take(5).ToList();

        var topCustomerStats = topCustomersRaw
            .Select(x => new TopCustomerStat(
                x.CustomerId,
                x.Name ?? "Customer",
                NormalizeSegment(x.Segment, unspecifiedSegmentLabel),
                x.InteractionCount,
                x.LastInteractionAt)).ToList();

        return new CustomerDashboardData(customerListItems, totalCustomers, newCustomersCount,
            recentInteractionsCount, distinctSegmentCount, topSegmentName, topSegmentCustomerCount,
            segmentStats, topCustomerStats, labels, values);
    }

    private static string NormalizeSegment(string? segment, string unspecifiedLabel) =>
        string.IsNullOrWhiteSpace(segment) ? unspecifiedLabel : segment.Trim();
}

