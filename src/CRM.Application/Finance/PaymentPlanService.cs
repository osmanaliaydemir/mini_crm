using CRM.Application.Common;
using CRM.Application.Common.Pagination;
using CRM.Domain.Finance;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

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

    public async Task<PagedResult<PaymentPlanListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? customerSearch = null, CancellationToken cancellationToken = default)
    {
        return await GetAllPagedAsync(pagination, customerSearch, null, null, cancellationToken);
    }

    public async Task<PagedResult<PaymentPlanListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? search, string? sortColumn, string? sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _context.PaymentPlans.AsNoTracking();

        // Search: Tüm alanlarda arama (Id, Customer, Shipment, PlanType, TotalAmount, StartDate)
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Önce tüm olası eşleşmeleri bul
            var matchingPlanIds = new HashSet<Guid>();
            
            // Plan ID search (Guid parse edilebilirse)
            if (Guid.TryParse(search, out var planId))
            {
                matchingPlanIds.Add(planId);
            }
            
            // Customer search
            var filteredCustomers = await _context.Customers.AsNoTracking()
                .Where(c => c.Name.Contains(search) || (c.LegalName != null && c.LegalName.Contains(search)))
                .Select(c => c.Id).ToListAsync(cancellationToken);
            
            if (filteredCustomers.Count > 0)
            {
                var customerIdsSet = filteredCustomers.ToHashSet();
                var plansByCustomer = await _context.PaymentPlans.AsNoTracking()
                    .Where(p => customerIdsSet.Contains(p.CustomerId))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
                foreach (var id in plansByCustomer)
                {
                    matchingPlanIds.Add(id);
                }
            }
            
            // Shipment search
            var filteredShipments = await _context.Shipments.AsNoTracking()
                .Where(s => s.ReferenceNumber.Contains(search))
                .Select(s => s.Id).ToListAsync(cancellationToken);
            
            if (filteredShipments.Count > 0)
            {
                var shipmentIdsSet = filteredShipments.ToHashSet();
                var plansByShipment = await _context.PaymentPlans.AsNoTracking()
                    .Where(p => shipmentIdsSet.Contains(p.ShipmentId))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
                foreach (var id in plansByShipment)
                {
                    matchingPlanIds.Add(id);
                }
            }
            
            // PlanType enum search
            var planTypeMatch = Enum.GetValues<PaymentPlanType>()
                .FirstOrDefault(pt => pt.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
            
            if (planTypeMatch != default(PaymentPlanType))
            {
                var plansByType = await _context.PaymentPlans.AsNoTracking()
                    .Where(p => p.PlanType == planTypeMatch)
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
                foreach (var id in plansByType)
                {
                    matchingPlanIds.Add(id);
                }
            }
            
            // Decimal search
            if (decimal.TryParse(search, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                var plansByAmount = await _context.PaymentPlans.AsNoTracking()
                    .Where(p => p.TotalAmount == amount)
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
                foreach (var id in plansByAmount)
                {
                    matchingPlanIds.Add(id);
                }
            }
            
            // Date search
            if (DateTime.TryParse(search, out var searchDate))
            {
                var dateStart = searchDate.Date;
                var dateEnd = searchDate.Date.AddDays(1);
                var plansByDate = await _context.PaymentPlans.AsNoTracking()
                    .Where(p => p.StartDate >= dateStart && p.StartDate < dateEnd)
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
                foreach (var id in plansByDate)
                {
                    matchingPlanIds.Add(id);
                }
            }
            
            // Tüm eşleşen plan ID'lerine göre filtrele
            if (matchingPlanIds.Count > 0)
            {
                var ids = matchingPlanIds;
                query = query.Where(p => ids.Contains(p.Id));
            }
            else
            {
                // No matches found, return empty query
                query = query.Where(p => false);
            }
        }

        // Sorting
        var isDescending = sortDirection?.ToLower() == "desc";
        
        // Customer ve Shipment sorting için özel işlem gerekli
        // Bu durumlarda önce tüm verileri çekip memory'de sıralama yapacağız
        if (sortColumn?.ToLower() == "customername" || sortColumn?.ToLower() == "shipmentreference")
        {
            // Önce tüm verileri çek (pagination olmadan)
            var allPlans = await query.ToListAsync(cancellationToken);
            
            // Customer ve Shipment bilgilerini çek
            var allCustomerIds = allPlans.Select(p => p.CustomerId).Distinct().ToHashSet();
            var allShipmentIds = allPlans.Select(p => p.ShipmentId).Distinct().ToHashSet();
            
            var allCustomers = await _context.Customers.AsNoTracking()
                .Where(c => allCustomerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
            
            var allShipments = await _context.Shipments.AsNoTracking()
                .Where(s => allShipmentIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.ReferenceNumber, cancellationToken);
            
            // Memory'de sıralama yap
            var sortedPlans = sortColumn.ToLower() switch
            {
                "customername" => isDescending
                    ? allPlans.OrderByDescending(p => allCustomers.GetValueOrDefault(p.CustomerId, ""))
                        .ThenByDescending(p => p.Id)
                    : allPlans.OrderBy(p => allCustomers.GetValueOrDefault(p.CustomerId, ""))
                        .ThenBy(p => p.Id),
                "shipmentreference" => isDescending
                    ? allPlans.OrderByDescending(p => allShipments.GetValueOrDefault(p.ShipmentId, ""))
                        .ThenByDescending(p => p.Id)
                    : allPlans.OrderBy(p => allShipments.GetValueOrDefault(p.ShipmentId, ""))
                        .ThenBy(p => p.Id),
                _ => allPlans.OrderByDescending(p => p.StartDate)
            };
            
            // Pagination uygula
            var totalCount = sortedPlans.Count();
            var pagedItems = sortedPlans
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();
            
            var pagedResult = new PagedResult<PaymentPlan>(pagedItems, totalCount, pagination.PageNumber, pagination.PageSize);
            
            // Customer ve Shipment bilgilerini tekrar çek (paged items için)
            var pagedCustomerIds = pagedResult.Items.Select(p => p.CustomerId).Distinct().ToHashSet();
            var pagedShipmentIds = pagedResult.Items.Select(p => p.ShipmentId).Distinct().ToHashSet();
            
            var pagedCustomers = await _context.Customers.AsNoTracking()
                .Where(c => pagedCustomerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
            
            var pagedShipments = await _context.Shipments.AsNoTracking()
                .Where(s => pagedShipmentIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.ReferenceNumber, cancellationToken);
            
            var dtoItems = pagedResult.Items.Select(plan => new PaymentPlanListItemDto(plan.Id, plan.CustomerId, plan.ShipmentId,
                plan.PlanType, plan.TotalAmount, plan.Currency, plan.StartDate,
                pagedCustomers.GetValueOrDefault(plan.CustomerId, "-"),
                pagedShipments.GetValueOrDefault(plan.ShipmentId, "-"))).ToList();
            
            return new PagedResult<PaymentPlanListItemDto>(dtoItems, totalCount, pagination.PageNumber, pagination.PageSize);
        }
        
        // Diğer kolonlar için normal sorting
        IQueryable<PaymentPlan> sortedQuery = sortColumn?.ToLower() switch
        {
            "id" => isDescending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
            "plantype" => isDescending ? query.OrderByDescending(p => p.PlanType) : query.OrderBy(p => p.PlanType),
            "totalamount" => isDescending ? query.OrderByDescending(p => p.TotalAmount) : query.OrderBy(p => p.TotalAmount),
            "startdate" => isDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
            _ => query.OrderByDescending(p => p.StartDate) // Default sorting
        };

        var pagedResultNormal = await sortedQuery.ToPagedResultAsync(pagination, cancellationToken);

        var customerIds = pagedResultNormal.Items.Select(p => p.CustomerId).Distinct().ToHashSet();
        var shipmentIds = pagedResultNormal.Items.Select(p => p.ShipmentId).Distinct().ToHashSet();

        // Fetch all relevant customers and shipments
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

        var items = pagedResultNormal.Items.Select(plan => new PaymentPlanListItemDto(plan.Id, plan.CustomerId, plan.ShipmentId,
            plan.PlanType, plan.TotalAmount, plan.Currency, plan.StartDate,
            customers.TryGetValue(plan.CustomerId, out var customerName) ? customerName : "-",
            shipments.TryGetValue(plan.ShipmentId, out var shipmentRef) ? shipmentRef : "-")).ToList();

        return new PagedResult<PaymentPlanListItemDto>(items, pagedResultNormal.TotalCount, pagedResultNormal.PageNumber, pagedResultNormal.PageSize);
    }

    // Expression tree helper - parameter replacement için
    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
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

