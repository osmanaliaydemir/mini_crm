using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Finance.PaymentPlans;

public class IndexModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public IndexModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string? CustomerFilter { get; set; }
    public IList<PaymentPlanListItem> Plans { get; private set; } = new List<PaymentPlanListItem>();

    public async Task OnGetAsync(string? customer, CancellationToken cancellationToken)
    {
        CustomerFilter = customer;

        var plansQuery = _dbContext.PaymentPlans.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(customer))
        {
            plansQuery = plansQuery.Where(plan =>
                _dbContext.Customers.Any(c => c.Id == plan.CustomerId && (EF.Functions.Like(c.Name, $"%{customer}%") || EF.Functions.Like(c.LegalName!, $"%{customer}%"))));
        }

        Plans = await plansQuery
            .OrderByDescending(plan => plan.StartDate)
            .Select(plan => new PaymentPlanListItem
            {
                Id = plan.Id,
                CustomerId = plan.CustomerId,
                ShipmentId = plan.ShipmentId,
                PlanType = plan.PlanType.ToString(),
                TotalAmount = plan.TotalAmount,
                Currency = plan.Currency,
                StartDate = plan.StartDate,
                CustomerName = _dbContext.Customers.Where(c => c.Id == plan.CustomerId).Select(c => c.Name).FirstOrDefault() ?? "-",
                ShipmentReference = _dbContext.Shipments.Where(s => s.Id == plan.ShipmentId).Select(s => s.ReferenceNumber).FirstOrDefault() ?? "-"
            })
            .ToListAsync(cancellationToken);
    }

    public sealed class PaymentPlanListItem
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ShipmentId { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "TRY";
        public DateTime StartDate { get; set; }
        public string CustomerName { get; set; } = "-";
        public string ShipmentReference { get; set; } = "-";
    }
}

