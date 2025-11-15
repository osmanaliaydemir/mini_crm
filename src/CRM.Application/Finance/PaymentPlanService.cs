using CRM.Application.Common;
using CRM.Domain.Finance;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Finance;

public class PaymentPlanService : IPaymentPlanService
{
    private readonly IRepository<PaymentPlan> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentPlanService(IRepository<PaymentPlan> repository, IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentPlanDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.PaymentPlans.AsNoTracking().Include(p => p.Installments).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan == null)
        {
            return null;
        }

        var customerName = await _context.Customers.AsNoTracking().Where(c => c.Id == plan.CustomerId)
            .Select(c => c.Name).FirstOrDefaultAsync(cancellationToken) ?? "-";

        var shipmentReference = await _context.Shipments.AsNoTracking().Where(s => s.Id == plan.ShipmentId)
            .Select(s => s.ReferenceNumber).FirstOrDefaultAsync(cancellationToken) ?? "-";

        var planDto = new PaymentPlanDto(plan.Id, plan.CustomerId, plan.ShipmentId,
            plan.PlanType, plan.TotalAmount, plan.Currency, plan.StartDate,
            plan.PeriodicityWeeks, plan.Notes, customerName, shipmentReference, plan.CreatedAt, plan.RowVersion);

        var installments = plan.Installments.OrderBy(i => i.InstallmentNumber).Select(i => new PaymentInstallmentDto(
                i.Id,
                i.PaymentPlanId,
                i.InstallmentNumber,
                i.DueDate,
                i.PaidAt,
                i.Amount,
                i.Currency,
                i.PaidAmount,
                i.Notes))
            .ToList();

        return new PaymentPlanDetailsDto(planDto, installments);
    }

    public async Task<IReadOnlyList<PaymentPlanListItemDto>> GetAllAsync(string? customerSearch = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PaymentPlans.AsNoTracking();

        HashSet<Guid>? filteredCustomerIds = null;
        if (!string.IsNullOrWhiteSpace(customerSearch))
        {
            var filteredCustomers = await _context.Customers.AsNoTracking()
                .Where(c => c.Name.Contains(customerSearch) || (c.LegalName != null && c.LegalName.Contains(customerSearch)))
                .Select(c => c.Id).ToListAsync(cancellationToken);

            filteredCustomerIds = filteredCustomers.ToHashSet();
        }

        var plans = await query.OrderByDescending(plan => plan.StartDate).ToListAsync(cancellationToken);

        // Filter plans in memory if customer search was provided
        if (filteredCustomerIds != null && filteredCustomerIds.Count > 0)
        {
            plans = plans.Where(p => filteredCustomerIds.Contains(p.CustomerId)).ToList();
        }

        var customerIds = plans.Select(p => p.CustomerId).Distinct().ToHashSet();
        var shipmentIds = plans.Select(p => p.ShipmentId).Distinct().ToHashSet();

        // Fetch all relevant customers and shipments to avoid SQL parameter limits
        // Filter in memory instead of using Contains() in SQL query
        Dictionary<Guid, string> customers;
        if (customerIds.Count > 0)
        {
            var allCustomers = await _context.Customers.AsNoTracking().Select(c => new { c.Id, c.Name }).ToListAsync(cancellationToken);

            customers = allCustomers.Where(c => customerIds.Contains(c.Id)).ToDictionary(c => c.Id, c => c.Name);
        }
        else
        {
            customers = new Dictionary<Guid, string>();
        }

        Dictionary<Guid, string> shipments;
        if (shipmentIds.Count > 0)
        {
            var allShipments = await _context.Shipments.AsNoTracking().Select(s => new { s.Id, s.ReferenceNumber }).ToListAsync(cancellationToken);

            shipments = allShipments.Where(s => shipmentIds.Contains(s.Id)).ToDictionary(s => s.Id, s => s.ReferenceNumber);
        }
        else
        {
            shipments = new Dictionary<Guid, string>();
        }

        return plans.Select(plan => new PaymentPlanListItemDto(plan.Id, plan.CustomerId, plan.ShipmentId,
            plan.PlanType, plan.TotalAmount, plan.Currency, plan.StartDate,
            customers.TryGetValue(plan.CustomerId, out var customerName) ? customerName : "-",
            shipments.TryGetValue(plan.ShipmentId, out var shipmentRef) ? shipmentRef : "-")).ToList();
    }

    public async Task<Guid> CreateAsync(CreatePaymentPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = new PaymentPlan(Guid.NewGuid(), request.CustomerId, request.ShipmentId, request.PlanType,
            request.TotalAmount, request.Currency ?? "TRY", request.StartDate, request.PeriodicityWeeks);

        plan.Update(request.PlanType, request.TotalAmount, request.Currency ?? "TRY", request.StartDate, request.PeriodicityWeeks, request.Notes);

        if (request.Installments?.Any() == true)
        {
            var installments = request.Installments.Select(inst =>
                new PaymentInstallment(plan.Id, inst.InstallmentNumber, inst.DueDate, inst.Amount, request.Currency ?? "TRY"));

            plan.SetInstallments(installments);
        }

        await _repository.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}

