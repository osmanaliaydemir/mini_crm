using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Finance.PaymentPlans;

public class DetailsModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public DetailsModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public PaymentPlanView? Plan { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.PaymentPlans
            .AsNoTracking()
            .Include(p => p.Installments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan is null)
        {
            return NotFound();
        }

        Plan = new PaymentPlanView
        {
            Id = plan.Id,
            CustomerId = plan.CustomerId,
            ShipmentId = plan.ShipmentId,
            PlanType = plan.PlanType.ToString(),
            TotalAmount = plan.TotalAmount,
            Currency = plan.Currency,
            StartDate = plan.StartDate,
            PeriodicityWeeks = plan.PeriodicityWeeks,
            Notes = plan.Notes,
            CustomerName = await _dbContext.Customers.Where(c => c.Id == plan.CustomerId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken) ?? "-",
            ShipmentReference = await _dbContext.Shipments.Where(s => s.Id == plan.ShipmentId).Select(s => s.ReferenceNumber).FirstOrDefaultAsync(cancellationToken) ?? "-",
            Installments = plan.Installments
                .OrderBy(i => i.InstallmentNumber)
                .Select(i => new PaymentInstallmentView
                {
                    Id = i.Id,
                    PaymentPlanId = i.PaymentPlanId,
                    InstallmentNumber = i.InstallmentNumber,
                    DueDate = i.DueDate,
                    PaidAt = i.PaidAt,
                    Amount = i.Amount,
                    Currency = i.Currency,
                    PaidAmount = i.PaidAmount,
                    Notes = i.Notes
                })
                .ToList()
        };

        return Page();
    }

    public sealed class PaymentPlanView
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ShipmentId { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "TRY";
        public DateTime StartDate { get; set; }
        public int PeriodicityWeeks { get; set; }
        public string? Notes { get; set; }
        public string CustomerName { get; set; } = "-";
        public string ShipmentReference { get; set; } = "-";
        public IList<PaymentInstallmentView> Installments { get; set; } = new List<PaymentInstallmentView>();
    }

    public sealed class PaymentInstallmentView
    {
        public Guid Id { get; set; }
        public Guid PaymentPlanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public decimal? PaidAmount { get; set; }
        public string? Notes { get; set; }
    }
}

