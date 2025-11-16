using CRM.Application.Common;
using CRM.Application.Common.Caching;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Domain.Suppliers;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Suppliers;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        IRepository<Supplier> repository, 
        IApplicationDbContext context, 
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<SupplierService> logger)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (supplier == null) return null;
        return new SupplierDto(supplier.Id, supplier.Name, supplier.Country, supplier.TaxNumber,
            supplier.ContactEmail, supplier.ContactPhone, supplier.AddressLine, supplier.Notes,
            supplier.CreatedAt, supplier.RowVersion);
    }

    public async Task<IReadOnlyList<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await _context.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(cancellationToken);
        return suppliers.Select(s => new SupplierDto(s.Id, s.Name, s.Country, s.TaxNumber,
            s.ContactEmail, s.ContactPhone, s.AddressLine, s.Notes, s.CreatedAt, s.RowVersion)).ToList();
    }

    public async Task<PagedResult<SupplierDto>> GetAllPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var pagedResult = await _context.Suppliers.AsNoTracking()
            .OrderBy(s => s.Name)
            .ToPagedResultAsync(pagination, cancellationToken);

        var items = pagedResult.Items.Select(s => new SupplierDto(s.Id, s.Name, s.Country, s.TaxNumber,
            s.ContactEmail, s.ContactPhone, s.AddressLine, s.Notes, s.CreatedAt, s.RowVersion)).ToList();

        return new PagedResult<SupplierDto>(items, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<Guid> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier(Guid.NewGuid(), request.Name, request.Country,
            request.TaxNumber, request.ContactEmail, request.ContactPhone, request.AddressLine);

        supplier.Update(request.Name, request.Country, request.TaxNumber,
            request.ContactEmail, request.ContactPhone, request.AddressLine, request.Notes);

        await _repository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cacheService.RemoveAsync(CacheKeys.SupplierDashboard, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);

        return supplier.Id;
    }

    public async Task UpdateAsync(UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null)
        {
            throw new NotFoundException(nameof(Supplier), request.Id);
        }

        // Set RowVersion for optimistic concurrency control
        supplier.RowVersion = request.RowVersion;

        supplier.Update(request.Name, request.Country, request.TaxNumber,
            request.ContactEmail, request.ContactPhone, request.AddressLine, request.Notes);

        await _repository.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cacheService.RemoveAsync(CacheKeys.SupplierDashboard, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetByIdAsync(id, cancellationToken);
        if (supplier == null)
        {
            throw new NotFoundException(nameof(Supplier), id);
        }

        await _repository.DeleteAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cacheService.RemoveAsync(CacheKeys.SupplierDashboard, cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DashboardData, cancellationToken);
    }

    public async Task<SupplierDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(
            CacheKeys.SupplierDashboard,
            async () => await LoadSupplierDashboardDataAsync(cancellationToken),
            TimeSpan.FromMinutes(5),
            cancellationToken);
    }

    private async Task<SupplierDashboardData> LoadSupplierDashboardDataAsync(CancellationToken cancellationToken)
    {
        var suppliers = await _context.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(cancellationToken);

        var supplierDtos = suppliers.Select(s => new SupplierDto(s.Id, s.Name, s.Country, s.TaxNumber,
            s.ContactEmail, s.ContactPhone, s.AddressLine, s.Notes, s.CreatedAt, s.RowVersion)).ToList();
        var totalSuppliers = suppliers.Count;

        var countryComparer = StringComparer.OrdinalIgnoreCase;
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        const string unspecifiedCountryLabel = "Belirtilmedi";

        var countryStats = suppliers.GroupBy(s => NormalizeCountry(s.Country, unspecifiedCountryLabel), countryComparer)
            .Select(group => new SupplierCountryStat(group.Key, group.Count())).OrderByDescending(stat => stat.SupplierCount)
            .ThenBy(stat => stat.Country, countryComparer).ToList();

        var distinctCountryCount = countryStats.Count;
        var recentSuppliersCount = suppliers.Count(s => s.CreatedAt >= thirtyDaysAgo);

        var topCountry = countryStats.FirstOrDefault();
        var topCountryName = topCountry?.Country ?? "-";
        var topCountrySupplierCount = topCountry?.SupplierCount ?? 0;

        var labels = countryStats.Select(stat => stat.Country).ToList();
        var values = countryStats.Select(stat => stat.SupplierCount).ToList();

        return new SupplierDashboardData(supplierDtos, totalSuppliers,
            recentSuppliersCount, distinctCountryCount, topCountryName, topCountrySupplierCount,
            countryStats, labels, values);
    }

    private static string NormalizeCountry(string? country, string unspecifiedLabel) =>
        string.IsNullOrWhiteSpace(country) ? unspecifiedLabel : country.Trim();
}

