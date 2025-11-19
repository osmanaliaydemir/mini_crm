using CRM.Application.Common.Pagination;

namespace CRM.Application.Products;

public interface IProductService
{
    Task<LumberVariantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LumberVariantListItemDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<PagedResult<LumberVariantListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? search = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateLumberVariantRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateLumberVariantRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

