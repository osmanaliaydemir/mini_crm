using CRM.Application.Common;
using CRM.Application.Notifications.Automation;
using CRM.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scheduling;

public class EmailAutomationBootstrapper : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailAutomationBootstrapper> _logger;

    public EmailAutomationBootstrapper(
        IServiceProvider serviceProvider,
        ILogger<EmailAutomationBootstrapper> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IEmailAutomationScheduler>();

        var scheduledRules = await context.EmailAutomationRules
            .AsNoTracking()
            .Where(rule => rule.ExecutionType == EmailExecutionType.Scheduled && rule.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var rule in scheduledRules)
        {
            await scheduler.ScheduleAsync(rule, cancellationToken);
        }

        _logger.LogInformation("Email automation bootstrapper scheduled {Count} rules.", scheduledRules.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

