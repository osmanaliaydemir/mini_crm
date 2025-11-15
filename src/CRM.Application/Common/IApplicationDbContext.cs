using CRM.Domain.Customers;
using CRM.Domain.Finance;
using CRM.Domain.Shipments;
using CRM.Domain.Suppliers;
using CRM.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Common;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<CustomerContact> CustomerContacts { get; }
    DbSet<CustomerInteraction> CustomerInteractions { get; }
    DbSet<Shipment> Shipments { get; }
    DbSet<CashTransaction> CashTransactions { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<WarehouseUnloading> WarehouseUnloadings { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<PaymentPlan> PaymentPlans { get; }
    DbSet<PaymentInstallment> PaymentInstallments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

