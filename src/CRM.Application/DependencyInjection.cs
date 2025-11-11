using Microsoft.Extensions.DependencyInjection;

namespace CRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // TODO: Application servisleri burada kaydedilecek.
        return services;
    }
}

