using CRM.Application.Common.Pagination;

namespace CRM.Application.Users;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<UserDto>> GetAllPagedAsync(PaginationRequest pagination, string? search = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAllRolesAsync(CancellationToken cancellationToken = default);
}

