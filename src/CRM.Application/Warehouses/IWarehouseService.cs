using CRM.Application.Common.Pagination;

namespace CRM.Application.Warehouses;

public interface IWarehouseService
{
    Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<WarehouseDto>> GetAllPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateWarehouseRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WarehouseDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> AddUnloadingAsync(AddUnloadingRequest request, CancellationToken cancellationToken = default);
}

