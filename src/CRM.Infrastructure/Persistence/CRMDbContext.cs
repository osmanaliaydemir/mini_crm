using CRM.Infrastructure.Identity;
using CRM.Domain.Abstractions;
using CRM.Domain.Products;
using CRM.Domain.Suppliers;
using CRM.Domain.Shipments;
using CRM.Domain.Warehouses;
using CRM.Domain.Customers;
using CRM.Domain.Finance;
using CRM.Domain.Notifications;
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
