using CRM.Application.Common.Pagination;

namespace CRM.Application.Shipments;

public interface IShipmentService
{
    Task<ShipmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShipmentDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShipmentListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ShipmentListItemDto>> GetAllPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateShipmentRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateShipmentRequest request, CancellationToken cancellationToken = default);
    Task<ShipmentDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}

