using CRM.Application.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Identity;

public class UserDirectory : IUserDirectory
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserDirectory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<string?> GetEmailByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email;
    }

    public async Task<string?> GetEmailByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var user = await _userManager.FindByNameAsync(userName);
        return user?.Email;
    }

    public async Task<IReadOnlyList<string>> GetEmailsByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
        {
            return Array.Empty<string>();
        }

        var users = _userManager.Users.Where(u => ids.Contains(u.Id));
        var emails = await Task.Run(
            () => users
                .Select(u => u.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email!)
                .ToList(),
            cancellationToken);

        return emails;
    }

    public async Task<IReadOnlyList<string>> GetAllUserEmailsAsync(CancellationToken cancellationToken = default)
    {
        var users = _userManager.Users.Select(u => u.Email);
        var emails = await Task.Run(
            () => users
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email!)
                .ToList(),
            cancellationToken);

        return emails;
    }

    public async Task<IReadOnlyList<UserDirectoryEntry>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);

        var result = new List<UserDirectoryEntry>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var displayName = string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                ? user.UserName ?? user.Email ?? "User"
                : $"{user.FirstName} {user.LastName}".Trim();
            result.Add(new UserDirectoryEntry(
                user.Id,
                displayName,
                user.Email,
                roles.ToList()));
        }

        return result;
    }

    public async Task<IReadOnlyList<string>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _roleManager.Roles
            .Select(r => r.Name!)
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDirectoryEntry>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return Array.Empty<UserDirectoryEntry>();
        }

        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users
            .Select(u =>
            {
                var displayName = string.IsNullOrWhiteSpace(u.FirstName) && string.IsNullOrWhiteSpace(u.LastName)
                    ? u.UserName ?? u.Email ?? "User"
                    : $"{u.FirstName} {u.LastName}".Trim();
                return new UserDirectoryEntry(
                    u.Id,
                    displayName,
                    u.Email,
                    new[] { roleName });
            })
            .ToList();
    }
}

