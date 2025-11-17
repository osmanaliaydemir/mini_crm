using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Application.Finance;
using CRM.Application.Customers;
using CRM.Application.Shipments;
using CRM.Domain.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Finance.PaymentPlans;

public class CreateModel : PageModel
{
    private readonly IPaymentPlanService _paymentPlanService;
    private readonly ICustomerService _customerService;
    private readonly IShipmentService _shipmentService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IPaymentPlanService paymentPlanService, ICustomerService customerService,
        IShipmentService shipmentService, ILogger<CreateModel> logger)
    {
        _paymentPlanService = paymentPlanService;
        _customerService = customerService;
        _shipmentService = shipmentService;
        _logger = logger;
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

        try
        {
            var installments = Input.Installments?.Select(inst => new CreatePaymentInstallmentRequest(
                inst.InstallmentNumber,
                inst.DueDate,
                inst.Amount)).ToList();

            var request = new CreatePaymentPlanRequest(
                Input.CustomerId,
                Input.ShipmentId,
                Input.PlanType,
                Input.TotalAmount,
                Input.Currency ?? "TRY",
                Input.StartDate,
                Input.PeriodicityWeeks,
                Input.Notes,
                installments);

            await _paymentPlanService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Ödeme planı başarıyla oluşturuldu.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment plan");
            ModelState.AddModelError(string.Empty, "Ödeme planı oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken: cancellationToken);
        CustomerOptions = customers
            .Select(customer => new SelectListItem(customer.Name, customer.Id.ToString()))
            .ToList();

        var shipments = await _shipmentService.GetAllAsync(cancellationToken: cancellationToken);
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

