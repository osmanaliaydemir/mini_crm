using CRM.Application.Finance;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Finance.PaymentPlans;

public class IndexModel : PageModel
{
    private readonly IPaymentPlanService _paymentPlanService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IPaymentPlanService paymentPlanService, ILogger<IndexModel> logger)
    {
        _paymentPlanService = paymentPlanService;
        _logger = logger;
    }

    public string? CustomerFilter { get; set; }
    public IReadOnlyList<PaymentPlanListItemDto> Plans { get; private set; } = Array.Empty<PaymentPlanListItemDto>();

    public async Task OnGetAsync(string? customer, CancellationToken cancellationToken)
    {
        CustomerFilter = customer;

        try
        {
            Plans = await _paymentPlanService.GetAllAsync(customer, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment plans");
            Plans = Array.Empty<PaymentPlanListItemDto>();
        }
    }
}

