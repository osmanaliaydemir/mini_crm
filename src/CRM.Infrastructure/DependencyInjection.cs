using CRM.Application.Authentication;
using CRM.Application.Common;
using CRM.Infrastructure.Email;
using CRM.Infrastructure.Identity;
using CRM.Infrastructure.Persistence;
using CRM.Application.Users;
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
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        });

        identityBuilder = new IdentityBuilder(identityBuilder.UserType, typeof(ApplicationRole), identityBuilder.Services);
        identityBuilder.AddRoles<ApplicationRole>();
        identityBuilder.AddEntityFrameworkStores<CRMDbContext>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CRMDbContext>());
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<CRMDbContext>());
        services.AddScoped<IEmailSender, EmailSender>();

        // Register repositories
        services.AddScoped(typeof(Persistence.Repositories.IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(CRM.Application.Common.IRepository<>), typeof(Repository<>));

        // Register application services
        services.AddScoped<CRM.Application.Users.IUserService, Identity.UserService>();

        return services;
    }
}

