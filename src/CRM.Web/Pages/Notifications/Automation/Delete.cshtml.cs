using CRM.Application.Notifications.Automation;
using CRM.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Notifications.Automation;

[Authorize(Policy = "AdminOnly")]
public class DeleteModel : PageModel
{
    private readonly IEmailAutomationService _emailAutomationService;
    private readonly ILogger<DeleteModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteModel(
        IEmailAutomationService emailAutomationService,
        ILogger<DeleteModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _emailAutomationService = emailAutomationService;
        _logger = logger;
        _localizer = localizer;
    }

    [BindProperty]
    public Guid RuleId { get; set; }

    public string? RuleName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _emailAutomationService.GetByIdAsync(id, cancellationToken);
            if (rule == null)
            {
                return NotFound();
            }

            RuleId = rule.Id;
            RuleName = rule.Name;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email automation rule for deletion. RuleId: {RuleId}", id);
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _emailAutomationService.DeleteRuleAsync(RuleId, cancellationToken);
            TempData["StatusMessage"] = _localizer["EmailAutomation_Message_Deleted"].Value;
            TempData["StatusMessageType"] = "success";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email automation rule. RuleId: {RuleId}", RuleId);
            TempData["StatusMessage"] = _localizer["EmailAutomation_Error_DeleteFailed"].Value;
            TempData["StatusMessageType"] = "danger";

            // Rule bilgisini tekrar yükle
            var rule = await _emailAutomationService.GetByIdAsync(RuleId, cancellationToken);
            if (rule != null)
            {
                RuleName = rule.Name;
            }

            return Page();
        }
    }
}

