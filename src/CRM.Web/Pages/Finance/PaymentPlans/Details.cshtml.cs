using CRM.Application.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Finance.PaymentPlans;

public class DetailsModel : PageModel
{
    private readonly IPaymentPlanService _paymentPlanService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IPaymentPlanService paymentPlanService, ILogger<DetailsModel> logger)
    {
        _paymentPlanService = paymentPlanService;
        _logger = logger;
    }

    public PaymentPlanDetailsDto? Details { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Details = await _paymentPlanService.GetDetailsByIdAsync(id, cancellationToken);
        if (Details is null)
        {
            return NotFound();
        }

        return Page();
    }
}

