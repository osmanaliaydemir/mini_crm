using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRM.Infrastructure.Persistence;

public class CRMDbContextFactory : IDesignTimeDbContextFactory<CRMDbContext>
{
    public CRMDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CRMDbContext>();
        optionsBuilder.UseSqlServer(GetDefaultConnectionString());

        return new CRMDbContext(optionsBuilder.Options);
    }

    private static string GetDefaultConnectionString()
    {
        const string fallback = "Server=localhost;Database=CRMDb;Trusted_Connection=True;TrustServerCertificate=True;";
        var connectionString = Environment.GetEnvironmentVariable("CRM_CONNECTION_STRING");

        return string.IsNullOrWhiteSpace(connectionString) ? fallback : connectionString;
    }
}

