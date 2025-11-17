using CRM.Application.Notifications.Automation;
using CRM.Domain.Notifications;
using CRM.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Notifications.Automation;

[Authorize(Policy = "AdminOnly")]
public class DetailsModel : PageModel
{
    private readonly IEmailAutomationService _emailAutomationService;
    private readonly ILogger<DetailsModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DetailsModel(
        IEmailAutomationService emailAutomationService,
        ILogger<DetailsModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _emailAutomationService = emailAutomationService;
        _logger = logger;
        _localizer = localizer;
    }

    public EmailAutomationRuleDto? Rule { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Rule = await _emailAutomationService.GetByIdAsync(id, cancellationToken);
            if (Rule == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email automation rule details. RuleId: {RuleId}", id);
            return NotFound();
        }
    }
}

