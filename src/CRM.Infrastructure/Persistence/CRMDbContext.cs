using CRM.Infrastructure.Identity;
using CRM.Domain.Abstractions;
using CRM.Domain.Products;
using CRM.Domain.Suppliers;
using CRM.Domain.Shipments;
using CRM.Domain.Warehouses;
using CRM.Domain.Customers;
using CRM.Domain.Finance;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence;

public class CRMDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public CRMDbContext(DbContextOptions<CRMDbContext> options)
        : base(options)
    {
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CRMDbContext).Assembly);
        modelBuilder.Ignore<DomainEvent>();
    }
}
