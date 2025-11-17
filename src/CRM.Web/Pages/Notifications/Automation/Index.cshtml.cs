using CRM.Application.Notifications.Automation;
using CRM.Domain.Notifications;
using CRM.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Notifications.Automation;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IEmailAutomationService _emailAutomationService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(
        IEmailAutomationService emailAutomationService,
        ILogger<IndexModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _emailAutomationService = emailAutomationService;
        _logger = logger;
        _localizer = localizer;
    }

    public IReadOnlyList<EmailAutomationRuleDto> Rules { get; private set; } = Array.Empty<EmailAutomationRuleDto>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Rules = await _emailAutomationService.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email automation rules");
            Rules = Array.Empty<EmailAutomationRuleDto>();
        }
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _emailAutomationService.GetByIdAsync(id, cancellationToken);
            if (rule == null)
            {
                TempData["StatusMessage"] = _localizer["EmailAutomation_Error_StatusUpdateFailed"].Value;
                TempData["StatusMessageType"] = "danger";
                return RedirectToPage();
            }

            var request = new UpdateEmailAutomationRuleRequest
            {
                Id = rule.Id,
                Name = rule.Name,
                ResourceType = rule.ResourceType,
                TriggerType = rule.TriggerType,
                ExecutionType = rule.ExecutionType,
                TemplateKey = rule.TemplateKey,
                CronExpression = rule.CronExpression,
                TimeZoneId = rule.TimeZoneId,
                RelatedEntityId = rule.RelatedEntityId,
                IsActive = !rule.IsActive,
                Metadata = rule.Metadata,
                Recipients = rule.Recipients.Select(r => new EmailAutomationRecipientRequest
                {
                    RecipientType = r.RecipientType,
                    UserId = r.UserId,
                    EmailAddress = r.EmailAddress,
                    RoleName = r.RoleName
                }).ToList(),
                RowVersion = rule.RowVersion
            };

            await _emailAutomationService.UpdateRuleAsync(request, cancellationToken);
            TempData["StatusMessage"] = _localizer["EmailAutomation_Message_StatusUpdated"].Value;
            TempData["StatusMessageType"] = "success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kural durumu değiştirilemedi. RuleId: {RuleId}", id);
            TempData["StatusMessage"] = _localizer["EmailAutomation_Error_StatusUpdateFailed"].Value;
            TempData["StatusMessageType"] = "danger";
        }

        return RedirectToPage();
    }
}

