using CRM.Application.Common.Pagination;

namespace CRM.Application.Suppliers;

public interface ISupplierService
{
    Task<SupplierDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<SupplierDto>> GetAllPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateSupplierRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SupplierDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}

