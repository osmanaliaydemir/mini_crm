using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Application.Users;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        // Kullanıcının oluşturulma zamanını almak için UserClaims veya ayrı bir tablo kullanılabilir
        // Şimdilik varsayılan değer kullanıyoruz
        var createdAt = user.LockoutEnd?.DateTime ?? DateTimeOffset.UtcNow;

        return new UserDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Locale,
            user.EmailConfirmed,
            user.LockoutEnabled,
            user.LockoutEnd,
            user.AccessFailedCount,
            createdAt,
            roles.ToList());
    }

    public async Task<PagedResult<UserDto>> GetAllPagedAsync(PaginationRequest pagination, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(searchLower)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var createdAt = user.LockoutEnd?.DateTime ?? DateTimeOffset.UtcNow;

            userDtos.Add(new UserDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Locale,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                user.AccessFailedCount,
                createdAt,
                roles.ToList()));
        }

        return new PagedResult<UserDto>(
            userDtos,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(searchLower)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower)));
        }

        var users = await query.OrderBy(u => u.UserName).ToListAsync(cancellationToken);
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var createdAt = user.LockoutEnd?.DateTime ?? DateTimeOffset.UtcNow;

            userDtos.Add(new UserDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Locale,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.LockoutEnd,
                user.AccessFailedCount,
                createdAt,
                roles.ToList()));
        }

        return userDtos;
    }

    public async Task<Guid> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser != null)
        {
            throw new BadRequestException("Bu kullanıcı adı zaten kullanılıyor.");
        }

        existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new BadRequestException("Bu e-posta adresi zaten kullanılıyor.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Locale = request.Locale,
            EmailConfirmed = true, // İlk oluşturmada otomatik onaylama
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Kullanıcı oluşturulamadı: {errors}");
        }

        // Rolleri ata
        if (request.Roles != null && request.Roles.Count > 0)
        {
            var validRoles = await GetAllRolesAsync(cancellationToken);
            var rolesToAssign = request.Roles.Where(r => validRoles.Contains(r)).ToList();
            
            if (rolesToAssign.Count > 0)
            {
                var roleResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Kullanıcı {UserId} için roller atanamadı: {Errors}",
                        user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        _logger.LogInformation("Yeni kullanıcı oluşturuldu: {UserId} ({UserName})", user.Id, user.UserName);

        return user.Id;
    }

    public async Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        // E-posta değiştiyse kontrol et
        if (user.Email != request.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                throw new BadRequestException("Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
            }
            user.Email = request.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(request.Email);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Locale = request.Locale;
        user.EmailConfirmed = request.EmailConfirmed;
        user.LockoutEnabled = request.LockoutEnabled;
        user.LockoutEnd = request.LockoutEnd;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Kullanıcı güncellenemedi: {errors}");
        }

        // Rolleri güncelle
        if (request.Roles != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();

            var validRoles = await GetAllRolesAsync(cancellationToken);
            rolesToAdd = rolesToAdd.Where(r => validRoles.Contains(r)).ToList();

            if (rolesToRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    _logger.LogWarning("Kullanıcı {UserId} için roller kaldırılamadı: {Errors}",
                        user.Id, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                }
            }

            if (rolesToAdd.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    _logger.LogWarning("Kullanıcı {UserId} için roller eklenemedi: {Errors}",
                        user.Id, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                }
            }
        }

        _logger.LogInformation("Kullanıcı güncellendi: {UserId} ({UserName})", user.Id, user.UserName);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Kullanıcı silinemedi: {errors}");
        }

        _logger.LogInformation("Kullanıcı silindi: {UserId} ({UserName})", user.Id, user.UserName);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Parola değiştirilemedi: {errors}");
        }

        _logger.LogInformation("Kullanıcı parolası değiştirildi: {UserId}", user.Id);
    }

    public async Task<IReadOnlyList<string>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);
        return roles;
    }
}

