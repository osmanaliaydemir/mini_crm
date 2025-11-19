using CRM.Application.Analytics;
using CRM.Application.AuditLogs;
using CRM.Application.Common.Caching;
using CRM.Application.Customers;
using CRM.Application.Dashboard;
using CRM.Application.ExportImport;
using CRM.Application.Finance;
using CRM.Application.Notifications;
using CRM.Application.Notifications.Automation;
using CRM.Application.Products;
using CRM.Application.Settings;
using CRM.Application.Shipments;
using CRM.Application.Suppliers;
using CRM.Application.Tasks;
using CRM.Application.Users;
using CRM.Application.Warehouses;
using FluentValidation;
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

        // FluentValidation - Validator'ları register et
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Cache service
        services.AddScoped<ICacheService, MemoryCacheService>();

        // Application services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<ICashTransactionService, CashTransactionService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPaymentPlanService, PaymentPlanService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailAutomationService, EmailAutomationService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<ISystemSettingsService, SystemSettingsService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}

