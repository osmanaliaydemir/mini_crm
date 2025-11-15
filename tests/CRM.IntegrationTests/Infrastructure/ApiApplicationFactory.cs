using CRM.Infrastructure.Identity;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;
using CRM.Web;

namespace CRM.IntegrationTests.Infrastructure;

public class ApiApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "InMemory"
            };

            configurationBuilder.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<CRMDbContext>));
            services.RemoveAll(typeof(CRMDbContext));
            services.RemoveAll(typeof(IDbInitializer));

            services.AddDbContext<CRMDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTests");
            });
            services.AddSingleton<IDbInitializer, NoOpDbInitializer>();

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CRMDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            EnsureRole(roleManager, "Admin");
            EnsureRole(roleManager, "Personel");

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            EnsureAdminUser(userManager);
        });
    }

    private static void EnsureRole(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
        {
            return;
        }

        var result = roleManager.CreateAsync(new ApplicationRole { Name = roleName })
            .GetAwaiter().GetResult();

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Rol oluşturulamadı: {roleName}");
        }
    }

    private static void EnsureAdminUser(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@crm.local";
        const string password = "Admin123!";

        var user = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = userManager.CreateAsync(user, password).GetAwaiter().GetResult();
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Varsayılan admin kullanıcısı oluşturulamadı.");
            }
        }

        if (!userManager.IsInRoleAsync(user, "Admin").GetAwaiter().GetResult())
        {
            userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
        }
    }

    private sealed class NoOpDbInitializer : IDbInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

