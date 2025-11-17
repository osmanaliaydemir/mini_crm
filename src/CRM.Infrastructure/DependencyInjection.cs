using CRM.Application.Authentication;
using CRM.Application.Common;
using CRM.Application.Notifications.Automation;
using CRM.Application.Search;
using CRM.Application.Timeline;
using CRM.Application.Users;
using CRM.Infrastructure.Email;
using CRM.Infrastructure.Identity;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["DatabaseProvider"];

        if (string.Equals(databaseProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<CRMDbContext>(options =>
                options.UseInMemoryDatabase("CRM"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Default' is not configured.");
            }

            services.AddDbContext<CRMDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure();
                        sqlOptions.MigrationsAssembly(typeof(CRMDbContext).Assembly.FullName);
                    }));
        }

        var identityBuilder = services.AddIdentityCore<ApplicationUser>(options =>
        {
            // OWASP Password Policy Guidelines
            options.Password.RequiredLength = 12; // Minimum 12 karakter
            options.Password.RequireDigit = true; // En az 1 rakam
            options.Password.RequireLowercase = true; // En az 1 küçük harf
            options.Password.RequireUppercase = true; // En az 1 büyük harf
            options.Password.RequireNonAlphanumeric = true; // En az 1 özel karakter
            options.Password.RequiredUniqueChars = 4; // En az 4 farklı karakter

            // Account Lockout - Brute force koruması
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // 15 dakika kilit
            options.Lockout.MaxFailedAccessAttempts = 5; // 5 başarısız deneme
            options.Lockout.AllowedForNewUsers = true; // Yeni kullanıcılar için de aktif

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // İsteğe bağlı: Email doğrulama
        });

        identityBuilder = new IdentityBuilder(identityBuilder.UserType, typeof(ApplicationRole), identityBuilder.Services);
        identityBuilder.AddRoles<ApplicationRole>();
        identityBuilder.AddEntityFrameworkStores<CRMDbContext>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CRMDbContext>());
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<CRMDbContext>());
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddSingleton<IEmailTemplateService, EmbeddedEmailTemplateService>();

        // Register repositories
        services.AddScoped(typeof(Persistence.Repositories.IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(CRM.Application.Common.IRepository<>), typeof(Repository<>));

        // Register application services
        services.AddScoped<CRM.Application.Users.IUserService, Identity.UserService>();
        services.AddScoped<IUserDirectory, Identity.UserDirectory>();
        services.AddScoped<IEmailAutomationScheduler, Scheduling.QuartzEmailAutomationScheduler>();
        services.AddScoped<IActivityTimelineService, Timeline.ActivityTimelineService>();
        services.AddScoped<IGlobalSearchService, Search.GlobalSearchService>();
        services.AddHostedService<Scheduling.EmailAutomationBootstrapper>();

        return services;
    }
}

