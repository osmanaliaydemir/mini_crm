using CRM.Application.Notifications.Automation;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CRM.Infrastructure.Scheduling;

public class EmailAutomationQuartzJob : IJob
{
    public const string RuleIdKey = "EmailAutomationRuleId";

    private readonly IEmailAutomationService _emailAutomationService;
    private readonly ILogger<EmailAutomationQuartzJob> _logger;

    public EmailAutomationQuartzJob(IEmailAutomationService emailAutomationService, ILogger<EmailAutomationQuartzJob> logger)
    {
        _emailAutomationService = emailAutomationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!context.MergedJobDataMap.TryGetValue(RuleIdKey, out var value) || value is not string ruleIdString ||
            !Guid.TryParse(ruleIdString, out var ruleId))
        {
            _logger.LogWarning("Email automation job triggered without valid rule id.");
            return;
        }

        await _emailAutomationService.ExecuteScheduledRuleAsync(ruleId, context.CancellationToken);
    }
}

