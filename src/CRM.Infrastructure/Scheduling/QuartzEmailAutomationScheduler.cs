using CRM.Application.Notifications.Automation;
using CRM.Domain.Notifications;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CRM.Infrastructure.Scheduling;

public class QuartzEmailAutomationScheduler : IEmailAutomationScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzEmailAutomationScheduler> _logger;

    public QuartzEmailAutomationScheduler(
        ISchedulerFactory schedulerFactory,
        ILogger<QuartzEmailAutomationScheduler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task ScheduleAsync(EmailAutomationRule rule, CancellationToken cancellationToken = default)
    {
        if (rule.ExecutionType != EmailExecutionType.Scheduled ||
            string.IsNullOrWhiteSpace(rule.CronExpression))
        {
            return;
        }

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = BuildJobKey(rule.Id);
        var triggerKey = BuildTriggerKey(rule.Id);

        var jobData = new JobDataMap
        {
            { EmailAutomationQuartzJob.RuleIdKey, rule.Id.ToString() }
        };

        var job = JobBuilder.Create<EmailAutomationQuartzJob>()
            .WithIdentity(jobKey)
            .SetJobData(jobData)
            .Build();

        var timeZone = ResolveTimeZone(rule.TimeZoneId);

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .WithCronSchedule(rule.CronExpression, builder => builder.InTimeZone(timeZone))
            .ForJob(jobKey)
            .Build();

        await scheduler.ScheduleJob(job, new[] { trigger }, replace: true, cancellationToken);

        _logger.LogInformation("Scheduled email automation rule {RuleId} with cron {Cron}.", rule.Id, rule.CronExpression);
    }

    public async Task UnscheduleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = BuildJobKey(ruleId);

        if (await scheduler.CheckExists(jobKey, cancellationToken))
        {
            await scheduler.DeleteJob(jobKey, cancellationToken);
            _logger.LogInformation("Unschedule email automation rule {RuleId}", ruleId);
        }
    }

    private static JobKey BuildJobKey(Guid ruleId) => new($"EmailAutomationRule-{ruleId}");

    private static TriggerKey BuildTriggerKey(Guid ruleId) => new($"EmailAutomationRuleTrigger-{ruleId}");

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}

