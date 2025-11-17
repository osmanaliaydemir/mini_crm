using CRM.Infrastructure.Identity;
using CRM.Domain.Abstractions;
using CRM.Domain.Products;
using CRM.Domain.Suppliers;
using CRM.Domain.Shipments;
using CRM.Domain.Warehouses;
using CRM.Domain.Customers;
using CRM.Domain.Finance;
using CRM.Domain.Notifications;
using CRM.Domain.Audit;
using CRM.Domain.Settings;
using CRM.Domain.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using CRM.Application.Common;

namespace CRM.Infrastructure.Persistence;

public class CRMDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, CRM.Application.Common.IUnitOfWork, IApplicationDbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private IDbContextTransaction? _currentTransaction;

    public CRMDbContext(DbContextOptions<CRMDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<LumberVariant> LumberVariants => Set<LumberVariant>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<ShipmentTransportUnit> ShipmentTransportUnits => Set<ShipmentTransportUnit>();
    public DbSet<ShipmentStage> ShipmentStages => Set<ShipmentStage>();
    public DbSet<CustomsProcess> CustomsProcesses => Set<CustomsProcess>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseUnloading> WarehouseUnloadings => Set<WarehouseUnloading>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CustomerInteraction> CustomerInteractions => Set<CustomerInteraction>();
    public DbSet<PaymentPlan> PaymentPlans => Set<PaymentPlan>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();
    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();
    public DbSet<EmailAutomationRule> EmailAutomationRules => Set<EmailAutomationRule>();
    public DbSet<EmailAutomationRuleRecipient> EmailAutomationRuleRecipients => Set<EmailAutomationRuleRecipient>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<TaskDb> Tasks => Set<TaskDb>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CRMDbContext).Assembly);
        modelBuilder.Ignore<DomainEvent>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync();
        UpdateAuditableEntities();
        await CreateAuditLogsAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = currentUser;
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedBy = currentUser;
                entry.Entity.LastModifiedAt = DateTime.UtcNow;
            }
        }
    }

    private async Task CreateAuditLogsAsync()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var userId = httpContext.User?.Identity?.Name;
        var userName = userId;
        var ipAddress = httpContext.Connection?.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

        // Identity user ID'sini al
        var userIdClaim = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userIdClaim))
        {
            userId = userIdClaim;
        }

        var entries = ChangeTracker.Entries<Entity<Guid>>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Where(e => !(e.Entity is AuditLog)) // AuditLog'un kendisini loglamıyoruz
            .ToList();

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Entity.Id;
            string action;
            string? changes = null;

            if (entry.State == EntityState.Added)
            {
                action = "Created";
                // Yeni entity için tüm property'leri JSON olarak kaydet
                changes = SerializeEntityProperties(entry.Entity);
            }
            else if (entry.State == EntityState.Modified)
            {
                action = "Updated";
                // Değişen property'leri kaydet
                var changedProperties = entry.Properties
                    .Where(p => p.IsModified && !p.Metadata.Name.EndsWith("RowVersion", StringComparison.OrdinalIgnoreCase))
                    .Select(p => new
                    {
                        Property = p.Metadata.Name,
                        OldValue = p.OriginalValue?.ToString(),
                        NewValue = p.CurrentValue?.ToString()
                    })
                    .ToList();

                if (changedProperties.Any())
                {
                    changes = System.Text.Json.JsonSerializer.Serialize(changedProperties);
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                action = "Deleted";
                // Silinen entity'nin son değerlerini kaydet
                changes = SerializeEntityProperties(entry.Entity);
            }
            else
            {
                continue;
            }

            var auditLog = new AuditLog(
                entityType,
                entityId,
                action,
                userId,
                userName,
                changes,
                ipAddress,
                userAgent);

            AuditLogs.Add(auditLog);
        }

        await Task.CompletedTask;
    }

    private static string SerializeEntityProperties(object entity)
    {
        try
        {
            var properties = entity.GetType().GetProperties()
                .Where(p => p.CanRead && !p.Name.EndsWith("RowVersion", StringComparison.OrdinalIgnoreCase))
                .Where(p => !p.PropertyType.IsGenericType || !typeof(System.Collections.ICollection).IsAssignableFrom(p.PropertyType))
                .ToDictionary(p => p.Name, p =>
                {
                    try
                    {
                        var value = p.GetValue(entity);
                        return value?.ToString() ?? "null";
                    }
                    catch
                    {
                        return "N/A";
                    }
                });

            return System.Text.Json.JsonSerializer.Serialize(properties);
        }
        catch
        {
            return "{}";
        }
    }

    private async Task DispatchDomainEventsAsync()
    {
        var entitiesWithEvents = ChangeTracker.Entries<Entity<Guid>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToArray();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                // Domain event handler'lar burada çağrılacak
                // Şimdilik sadece log yapıyoruz
                await Task.CompletedTask;
            }
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }
}
