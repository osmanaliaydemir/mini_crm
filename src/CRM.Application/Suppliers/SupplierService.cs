using CRM.Application.Common;
using CRM.Domain.Suppliers;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Suppliers;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IRepository<Supplier> repository, IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        return supplier?.Adapt<SupplierDto>();
    }

    public async Task<IReadOnlyList<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await _context.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(cancellationToken);
        return suppliers.Adapt<List<SupplierDto>>();
    }

    public async Task<Guid> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier(Guid.NewGuid(), request.Name, request.Country,
            request.TaxNumber, request.ContactEmail, request.ContactPhone, request.AddressLine);

        supplier.Update(request.Name, request.Country, request.TaxNumber,
            request.ContactEmail, request.ContactPhone, request.AddressLine, request.Notes);

        await _repository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return supplier.Id;
    }

    public async Task UpdateAsync(UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null)
        {
            throw new InvalidOperationException($"Supplier with id {request.Id} not found.");
        }

        supplier.Update(request.Name, request.Country, request.TaxNumber,
            request.ContactEmail, request.ContactPhone, request.AddressLine, request.Notes);

        await _repository.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetByIdAsync(id, cancellationToken);
        if (supplier == null)
        {
            throw new InvalidOperationException($"Supplier with id {id} not found.");
        }

        await _repository.DeleteAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SupplierDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await _context.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(cancellationToken);

        var supplierDtos = suppliers.Adapt<List<SupplierDto>>();
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

