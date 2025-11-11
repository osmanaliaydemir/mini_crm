using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Persistence;

public class DbInitializer : IDbInitializer
{
    private readonly CRMDbContext _context;
    private readonly ILogger<DbInitializer> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DbInitializer(
        CRMDbContext context,
        ILogger<DbInitializer> logger,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                await _context.Database.EnsureCreatedAsync(cancellationToken);
            }

            await EnsureRolesAsync();
            await EnsureAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veritabanı başlangıç işlemi başarısız oldu.");
            throw;
        }
    }

    private async Task EnsureRolesAsync()
    {
        await EnsureRoleExistsAsync("Admin");
        await EnsureRoleExistsAsync("Personel");
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await _roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName
        });

        if (!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            _logger.LogError("Rol oluşturulamadı: {Role}. Hatalar: {Errors}", roleName, errors);
            throw new InvalidOperationException($"Rol oluşturulamadı: {roleName}");
        }

        _logger.LogInformation("Rol oluşturuldu: {Role}", roleName);
    }

    private async Task EnsureAdminUserAsync()
    {
        const string adminEmail = "admin@crm.local";
        const string adminPassword = "Admin123!";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Sistem",
                LastName = "Yöneticisi",
                Locale = "tr-TR"
            };

            var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(",", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Varsayılan admin kullanıcısı oluşturulamadı. Hatalar: {Errors}", errors);
                throw new InvalidOperationException("Varsayılan admin kullanıcısı oluşturulamadı.");
            }

            _logger.LogInformation("Varsayılan admin kullanıcısı oluşturuldu: {Email}", adminEmail);
        }

        if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(",", addRoleResult.Errors.Select(e => e.Description));
                _logger.LogError("Admin rolü atanamadı. Hatalar: {Errors}", errors);
                throw new InvalidOperationException("Admin rolü atanamadı.");
            }
        }
    }
}

