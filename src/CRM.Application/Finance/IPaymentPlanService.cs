using CRM.Application.Common.Pagination;

namespace CRM.Application.Finance;

public interface IPaymentPlanService
{
    Task<PaymentPlanDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentPlanListItemDto>> GetAllAsync(string? customerSearch = null, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentPlanListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? customerSearch = null, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentPlanListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? search, string? sortColumn, string? sortDirection, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreatePaymentPlanRequest request, CancellationToken cancellationToken = default);
}

