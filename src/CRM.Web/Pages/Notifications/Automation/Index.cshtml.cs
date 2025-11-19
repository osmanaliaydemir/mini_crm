using System.Text.Encodings.Web;
using System.Text.Json;
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
    public string AutomationTableJson { get; private set; } = "[]";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Rules = await _emailAutomationService.GetAllAsync(cancellationToken);

            var tableRows = Rules.Select(MapToTableRow).ToList();
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            AutomationTableJson = JsonSerializer.Serialize(tableRows, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email automation rules");
            Rules = Array.Empty<EmailAutomationRuleDto>();
            AutomationTableJson = "[]";
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

    private AutomationRuleTableRow MapToTableRow(EmailAutomationRuleDto rule)
    {
        var recipientSummary = BuildRecipientSummary(rule);
        var statusLabel = rule.IsActive
            ? _localizer["EmailAutomation_Status_Active"].Value
            : _localizer["EmailAutomation_Status_Inactive"].Value;
        var toggleLabel = rule.IsActive
            ? _localizer["EmailAutomation_Action_Deactivate"].Value
            : _localizer["EmailAutomation_Action_Activate"].Value;

        return new AutomationRuleTableRow(
            Id: rule.Id,
            Name: rule.Name,
            CronExpression: rule.CronExpression ?? string.Empty,
            ResourceLabel: DescribeResource(rule.ResourceType),
            TriggerLabel: rule.TriggerType.ToString(),
            ExecutionLabel: DescribeExecution(rule.ExecutionType),
            RecipientSummary: recipientSummary,
            StatusLabel: statusLabel,
            StatusCssClass: rule.IsActive ? "success" : "inactive",
            DetailsUrl: Url.Page("Details", new { id = rule.Id }) ?? "#",
            EditUrl: Url.Page("Create", new { id = rule.Id }) ?? "#",
            DeleteUrl: Url.Page("Delete", new { id = rule.Id }) ?? "#",
            ToggleFormId: $"toggle-form-{rule.Id}",
            ToggleLabel: toggleLabel
        );
    }

    private string BuildRecipientSummary(EmailAutomationRuleDto rule)
    {
        var emailRecipients = rule.Recipients
            .Where(r => r.RecipientType == EmailRecipientType.CustomEmail && !string.IsNullOrWhiteSpace(r.EmailAddress))
            .Select(r => r.EmailAddress!.Trim())
            .ToList();

        var userCount = rule.Recipients.Count(r => r.RecipientType == EmailRecipientType.User && r.UserId.HasValue);

        var roleNames = rule.Recipients
            .Where(r => r.RecipientType == EmailRecipientType.Role && !string.IsNullOrWhiteSpace(r.RoleName))
            .Select(r => r.RoleName!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var summaryParts = new List<string>();

        if (emailRecipients.Count > 0)
        {
            summaryParts.Add(string.Join(", ", emailRecipients));
        }

        if (userCount > 0)
        {
            summaryParts.Add(_localizer["EmailAutomation_UserRecipient_Summary", userCount].Value);
        }

        if (roleNames.Count > 0)
        {
            summaryParts.Add(_localizer["EmailAutomation_RoleRecipient_Summary", string.Join(", ", roleNames)].Value);
        }

        return summaryParts.Count == 0
            ? _localizer["EmailAutomation_Table_NoRecipients"].Value
            : string.Join(" | ", summaryParts);
    }

    private string DescribeExecution(EmailExecutionType execution) => execution switch
    {
        EmailExecutionType.EventBased => _localizer["EmailAutomation_Execution_EventBased"].Value,
        EmailExecutionType.Scheduled => _localizer["EmailAutomation_Execution_Scheduled"].Value,
        _ => execution.ToString()
    };

    private string DescribeResource(EmailResourceType resource) => resource switch
    {
        EmailResourceType.Shipment => _localizer["EmailAutomation_Resource_Shipment"].Value,
        EmailResourceType.Finance => _localizer["EmailAutomation_Resource_Finance"].Value,
        EmailResourceType.Task => _localizer["EmailAutomation_Resource_Task"].Value,
        EmailResourceType.Customer => _localizer["EmailAutomation_Resource_Customer"].Value,
        EmailResourceType.Warehouse => _localizer["EmailAutomation_Resource_Warehouse"].Value,
        _ => resource.ToString()
    };

    private sealed record AutomationRuleTableRow(
        Guid Id,
        string Name,
        string CronExpression,
        string ResourceLabel,
        string TriggerLabel,
        string ExecutionLabel,
        string RecipientSummary,
        string StatusLabel,
        string StatusCssClass,
        string DetailsUrl,
        string EditUrl,
        string DeleteUrl,
        string ToggleFormId,
        string ToggleLabel);
}

