using CRM.Application.Common.Pagination;

namespace CRM.Application.Customers;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerListItemDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? search = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> AddInteractionAsync(Guid customerId, AddInteractionRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDashboardData> GetDashboardDataAsync(string? search = null, CancellationToken cancellationToken = default);
}

