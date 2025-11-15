using CRM.Application.Customers;
using CRM.Application.Finance;
using CRM.Application.Shipments;
using CRM.Application.Suppliers;
using CRM.Application.Warehouses;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mapster configuration
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(DependencyInjection).Assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, Mapper>();

        // Application services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<ICashTransactionService, CashTransactionService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPaymentPlanService, PaymentPlanService>();

        return services;
    }
}

