using System.ComponentModel.DataAnnotations;
using CRM.Domain.Finance;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Finance.PaymentPlans;

public class CreateModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public CreateModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public PaymentPlanInput Input { get; set; } = new();

    public IList<SelectListItem> CustomerOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> ShipmentOptions { get; private set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> PlanTypeOptions { get; private set; } =
        Enum.GetValues(typeof(PaymentPlanType))
            .Cast<PaymentPlanType>()
            .Select(type => new SelectListItem(type.ToString(), type.ToString()));

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var plan = new PaymentPlan(
            Guid.NewGuid(),
            Input.CustomerId,
            Input.ShipmentId,
            Input.PlanType,
            Input.TotalAmount,
            Input.Currency ?? "TRY",
            Input.StartDate,
            Input.PeriodicityWeeks);

        plan.Update(Input.PlanType, Input.TotalAmount, Input.Currency ?? "TRY", Input.StartDate, Input.PeriodicityWeeks, Input.Notes);

        if (Input.Installments?.Any() == true)
        {
            var installments = Input.Installments.Select(inst =>
                new PaymentInstallment(plan.Id, inst.InstallmentNumber, inst.DueDate, inst.Amount, Input.Currency ?? "TRY"));

            plan.SetInstallments(installments);
        }

        _dbContext.PaymentPlans.Add(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers
            .Select(customer => new { customer.Id, customer.Name })
            .ToListAsync(cancellationToken);

        CustomerOptions = customers
            .Select(customer => new SelectListItem(customer.Name, customer.Id.ToString()))
            .ToList();

        var shipments = await _dbContext.Shipments
            .Select(shipment => new { shipment.Id, shipment.ReferenceNumber })
            .ToListAsync(cancellationToken);

        ShipmentOptions = shipments
            .Select(shipment => new SelectListItem(shipment.ReferenceNumber, shipment.Id.ToString()))
            .ToList();

        if (Input.StartDate == default)
        {
            Input.StartDate = DateTime.UtcNow.Date;
        }

        if (Input.PeriodicityWeeks == 0)
        {
            Input.PeriodicityWeeks = 4;
        }
    }

    public sealed class PaymentPlanInput
    {
        [Display(Name = "Müşteri")]
        [Required]
        public Guid CustomerId { get; set; }

        [Display(Name = "Sevkiyat")]
        [Required]
        public Guid ShipmentId { get; set; }

        [Display(Name = "Plan Tipi")]
        [Required]
        public PaymentPlanType PlanType { get; set; } = PaymentPlanType.Installment;

        [Display(Name = "Toplam Tutar")]
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Para Birimi")]
        [MaxLength(10)]
        public string? Currency { get; set; } = "TRY";

        [Display(Name = "Başlangıç Tarihi")]
        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        [Display(Name = "Periyot (Hafta)")]
        [Range(1, 52)]
        public int PeriodicityWeeks { get; set; } = 4;

        [Display(Name = "Notlar")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<PaymentInstallmentInput>? Installments { get; set; }
    }

    public sealed class PaymentInstallmentInput
    {
        public int InstallmentNumber { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}

