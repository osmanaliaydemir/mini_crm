namespace CRM.Application.Common;

public interface IUserDirectory
{
    Task<string?> GetEmailByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string?> GetEmailByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEmailsByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAllUserEmailsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDirectoryEntry>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDirectoryEntry>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);
}

