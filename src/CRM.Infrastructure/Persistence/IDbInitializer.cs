namespace CRM.Infrastructure.Persistence;

public interface IDbInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

