using System.Globalization;
using System.Text;
using System.Text.Json;
using CRM.Application.Common;
using CRM.Domain.Finance;
using CRM.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Automation;

public class EmailAutomationService : IEmailAutomationService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _templateService;
    private readonly IUserDirectory _userDirectory;
    private readonly IEmailAutomationScheduler _scheduler;
    private readonly ILogger<EmailAutomationService> _logger;

    public EmailAutomationService(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IEmailTemplateService templateService,
        IUserDirectory userDirectory,
        IEmailAutomationScheduler scheduler,
        ILogger<EmailAutomationService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _templateService = templateService;
        _userDirectory = userDirectory;
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Guid> CreateRuleAsync(CreateEmailAutomationRuleRequest request, CancellationToken cancellationToken = default)
    {
        ValidateSchedule(request.ExecutionType, request.CronExpression, request.TimeZoneId);

        var ruleId = Guid.NewGuid();
        var rule = new EmailAutomationRule(
            ruleId,
            request.Name,
            request.ResourceType,
            request.TriggerType,
            request.ExecutionType,
            request.TemplateKey,
            request.CronExpression,
            request.TimeZoneId,
            request.RelatedEntityId,
            request.IsActive,
            request.Metadata);

        var recipients = BuildRecipients(ruleId, request.Recipients);
        rule.SetRecipients(recipients);

        await _context.EmailAutomationRules.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (rule.ExecutionType == EmailExecutionType.Scheduled && rule.IsActive)
        {
            await _scheduler.ScheduleAsync(rule, cancellationToken);
        }

        _logger.LogInformation("Email automation rule created: {RuleId} - {Name}", rule.Id, rule.Name);
        return rule.Id;
    }

    public async Task UpdateRuleAsync(UpdateEmailAutomationRuleRequest request, CancellationToken cancellationToken = default)
    {
        ValidateSchedule(request.ExecutionType, request.CronExpression, request.TimeZoneId);

        var rule = await _context.EmailAutomationRules
            .Include(r => r.Recipients)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule == null)
        {
            throw new InvalidOperationException($"Email automation rule {request.Id} not found.");
        }

        rule.RowVersion = request.RowVersion ?? Array.Empty<byte>();
        rule.Update(
            request.Name,
            request.ResourceType,
            request.TriggerType,
            request.ExecutionType,
            request.TemplateKey,
            request.CronExpression,
            request.TimeZoneId,
            request.RelatedEntityId,
            request.IsActive,
            request.Metadata);

        var recipients = BuildRecipients(rule.Id, request.Recipients);
        rule.SetRecipients(recipients);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _scheduler.UnscheduleAsync(rule.Id, cancellationToken);

        if (rule.ExecutionType == EmailExecutionType.Scheduled && rule.IsActive)
        {
            await _scheduler.ScheduleAsync(rule, cancellationToken);
        }

        _logger.LogInformation("Email automation rule updated: {RuleId} - {Name}", rule.Id, rule.Name);
    }

    public async Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.EmailAutomationRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule == null)
        {
            return;
        }

        _context.EmailAutomationRules.Remove(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _scheduler.UnscheduleAsync(id, cancellationToken);

        _logger.LogInformation("Email automation rule deleted: {RuleId}", id);
    }

    public async Task<EmailAutomationRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.EmailAutomationRules
            .AsNoTracking()
            .Include(r => r.Recipients)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return rule == null ? null : MapToDto(rule);
    }

    public async Task<IReadOnlyList<EmailAutomationRuleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _context.EmailAutomationRules
            .AsNoTracking()
            .Include(r => r.Recipients)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return rules.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<EmailAutomationRuleDto>> GetActiveRulesByExecutionTypeAsync(EmailExecutionType executionType, CancellationToken cancellationToken = default)
    {
        var rules = await _context.EmailAutomationRules
            .AsNoTracking()
            .Include(r => r.Recipients)
            .Where(r => r.ExecutionType == executionType && r.IsActive)
            .ToListAsync(cancellationToken);

        return rules.Select(MapToDto).ToList();
    }

    public async Task HandleEventAsync(EmailAutomationEventContext context, CancellationToken cancellationToken = default)
    {
        var query = _context.EmailAutomationRules
            .Include(r => r.Recipients)
            .Where(r => r.ExecutionType == EmailExecutionType.EventBased)
            .Where(r => r.IsActive)
            .Where(r => r.ResourceType == context.ResourceType && r.TriggerType == context.TriggerType);

        if (context.RelatedEntityId.HasValue)
        {
            var relatedId = context.RelatedEntityId.Value;
            query = query.Where(r => r.RelatedEntityId == null || r.RelatedEntityId == relatedId);
        }

        var rules = await query.ToListAsync(cancellationToken);
        var hasAnyRule = rules.Count > 0;

        if (!hasAnyRule && !context.ForceSendWhenNoRule)
        {
            return;
        }

        if (!hasAnyRule && context.ForceSendWhenNoRule)
        {
            await SendEmailsAsync(
                context.ResourceType,
                context.TriggerType,
                context.TemplateKey,
                context.Subject,
                context.Placeholders,
                Array.Empty<EmailAutomationRuleRecipient>(),
                context.AdditionalUserIds,
                context.AdditionalEmails,
                cancellationToken);
            return;
        }

        foreach (var rule in rules)
        {
            await SendEmailsAsync(
                rule.ResourceType,
                rule.TriggerType,
                rule.TemplateKey,
                string.IsNullOrWhiteSpace(context.Subject) ? rule.Name : context.Subject,
                context.Placeholders,
                rule.Recipients,
                context.AdditionalUserIds,
                context.AdditionalEmails,
                cancellationToken);
        }
    }

    public async Task ExecuteScheduledRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await _context.EmailAutomationRules
            .Include(r => r.Recipients)
            .FirstOrDefaultAsync(r => r.Id == ruleId, cancellationToken);

        if (rule == null)
        {
            _logger.LogWarning("Scheduled email rule not found: {RuleId}", ruleId);
            return;
        }

        if (!rule.IsActive)
        {
            _logger.LogInformation("Scheduled email rule disabled: {RuleId}", ruleId);
            return;
        }

        var placeholders = await BuildScheduledRulePlaceholdersAsync(rule, cancellationToken);

        await SendEmailsAsync(
            rule.ResourceType,
            rule.TriggerType,
            rule.TemplateKey,
            rule.Name,
            placeholders,
            rule.Recipients,
            Array.Empty<Guid>(),
            Array.Empty<string>(),
            cancellationToken);
    }

    private static void ValidateSchedule(EmailExecutionType executionType, string? cronExpression, string? timeZoneId)
    {
        if (executionType != EmailExecutionType.Scheduled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new InvalidOperationException("Zamanlanmış kurallar için cron ifadesi gereklidir.");
        }

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new InvalidOperationException("Zamanlanmış kurallar için saat dilimi gereklidir.");
        }
    }

    private static IReadOnlyCollection<EmailAutomationRuleRecipient> BuildRecipients(Guid ruleId, IEnumerable<EmailAutomationRecipientRequest> recipientRequests)
    {
        if (recipientRequests == null)
        {
            return Array.Empty<EmailAutomationRuleRecipient>();
        }

        return recipientRequests
            .Where(r => r != null)
            .Select(r => new EmailAutomationRuleRecipient(
                Guid.NewGuid(),
                ruleId,
                r.RecipientType,
                r.UserId,
                r.EmailAddress,
                r.RoleName))
            .ToList();
    }

    private EmailAutomationRuleDto MapToDto(EmailAutomationRule rule)
    {
        var recipients = rule.Recipients
            .Select(r => new EmailAutomationRecipientDto(r.Id, r.RecipientType, r.UserId, r.EmailAddress, r.RoleName))
            .ToList();

        return new EmailAutomationRuleDto(
            rule.Id,
            rule.Name,
            rule.ResourceType,
            rule.TriggerType,
            rule.ExecutionType,
            rule.TemplateKey,
            rule.CronExpression,
            rule.TimeZoneId,
            rule.RelatedEntityId,
            rule.IsActive,
            rule.Metadata,
            recipients,
            rule.CreatedAt,
            rule.CreatedBy,
            rule.LastModifiedAt,
            rule.LastModifiedBy,
            rule.RowVersion);
    }

    private async Task SendEmailsAsync(
        EmailResourceType resourceType,
        EmailTriggerType triggerType,
        string templateKey,
        string subject,
        IDictionary<string, string> placeholders,
        ICollection<EmailAutomationRuleRecipient>? ruleRecipients,
        IEnumerable<Guid> additionalUserIds,
        IEnumerable<string> additionalEmails,
        CancellationToken cancellationToken)
    {
        var recipients = await ResolveRecipientEmailsAsync(
            ruleRecipients,
            additionalUserIds,
            additionalEmails,
            resourceType,
            triggerType,
            cancellationToken);

        if (recipients.Count == 0)
        {
            _logger.LogWarning("Email automation skipped because there are no recipients.");
            return;
        }

        var templatePlaceholders = NormalizePlaceholders(placeholders);
        var body = await _templateService.RenderTemplateAsync(templateKey, templatePlaceholders, cancellationToken);

        foreach (var recipient in recipients)
        {
            try
            {
                await _emailSender.SendEmailAsync(recipient, subject, body, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email automation failed. Recipient: {Recipient}, Subject: {Subject}", recipient, subject);
            }
        }
    }

    private async Task<IReadOnlyList<string>> ResolveRecipientEmailsAsync(
        ICollection<EmailAutomationRuleRecipient>? ruleRecipients,
        IEnumerable<Guid> additionalUserIds,
        IEnumerable<string> additionalEmails,
        EmailResourceType resourceType,
        EmailTriggerType triggerType,
        CancellationToken cancellationToken)
    {
        var emailSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userIdsNeedingPreferences = new HashSet<Guid>();
        var roleUserEntries = new List<UserDirectoryEntry>();

        if (ruleRecipients != null)
        {
            foreach (var recipient in ruleRecipients)
            {
                switch (recipient.RecipientType)
                {
                    case EmailRecipientType.CustomEmail:
                        if (!string.IsNullOrWhiteSpace(recipient.EmailAddress))
                        {
                            emailSet.Add(recipient.EmailAddress);
                        }
                        break;
                    case EmailRecipientType.User when recipient.UserId.HasValue:
                        userIdsNeedingPreferences.Add(recipient.UserId.Value);
                        break;
                    case EmailRecipientType.Role when !string.IsNullOrWhiteSpace(recipient.RoleName):
                        var roleUsers = await _userDirectory.GetUsersByRoleAsync(recipient.RoleName, cancellationToken);
                        foreach (var user in roleUsers)
                        {
                            if (string.IsNullOrWhiteSpace(user.Email))
                            {
                                continue;
                            }

                            roleUserEntries.Add(user);
                            userIdsNeedingPreferences.Add(user.Id);
                        }
                        break;
                }
            }
        }

        if (additionalUserIds != null)
        {
            var ids = additionalUserIds.Where(id => id != Guid.Empty).Distinct().ToList();
            foreach (var id in ids)
            {
                userIdsNeedingPreferences.Add(id);
            }
        }

        var preferenceMap = await LoadPreferenceMapAsync(userIdsNeedingPreferences, resourceType, cancellationToken);

        if (additionalEmails != null)
        {
            foreach (var email in additionalEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    emailSet.Add(email);
                }
            }
        }

        if (ruleRecipients != null)
        {
            foreach (var recipient in ruleRecipients)
            {
                if (recipient.RecipientType == EmailRecipientType.User &&
                    recipient.UserId.HasValue &&
                    ShouldSendToUser(recipient.UserId.Value, preferenceMap, resourceType))
                {
                    var email = await _userDirectory.GetEmailByUserIdAsync(recipient.UserId.Value, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        emailSet.Add(email);
                    }
                }
            }
        }

        foreach (var user in roleUserEntries)
        {
            if (!ShouldSendToUser(user.Id, preferenceMap, resourceType))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                emailSet.Add(user.Email!);
            }
        }

        if (additionalUserIds != null)
        {
            foreach (var userId in additionalUserIds.Where(id => id != Guid.Empty))
            {
                if (!ShouldSendToUser(userId, preferenceMap, resourceType))
                {
                    continue;
                }

                var email = await _userDirectory.GetEmailByUserIdAsync(userId, cancellationToken);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    emailSet.Add(email);
                }
            }
        }

        return emailSet.ToList();
    }

    private async Task<IDictionary<string, string>> BuildScheduledRulePlaceholdersAsync(EmailAutomationRule rule, CancellationToken cancellationToken)
    {
        if (rule.ResourceType == EmailResourceType.Finance &&
            rule.TriggerType == EmailTriggerType.FinanceSummaryScheduled)
        {
            return await BuildFinanceSummaryPlaceholdersAsync(rule, cancellationToken);
        }

        return new Dictionary<string, string>
        {
            ["Title"] = rule.Name,
            ["Description"] = "Planlanmış e-posta bildirimi tetiklendi.",
            ["Content"] = rule.Metadata ?? string.Empty
        };
    }

    private async Task<IDictionary<string, string>> BuildFinanceSummaryPlaceholdersAsync(EmailAutomationRule rule, CancellationToken cancellationToken)
    {
        var metadata = ParseFinanceSummaryMetadata(rule.Metadata);
        var end = DateTime.UtcNow;
        var start = end.AddDays(-Math.Max(1, metadata.RangeDays));

        var transactions = await _context.CashTransactions
            .AsNoTracking()
            .Where(t => t.TransactionDate >= start && t.TransactionDate <= end)
            .ToListAsync(cancellationToken);

        var totalIncome = transactions
            .Where(t => t.TransactionType == CashTransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t => t.TransactionType == CashTransactionType.Expense)
            .Sum(t => t.Amount);

        var net = totalIncome - totalExpense;

        var topCategories = transactions
            .Where(t => !string.IsNullOrWhiteSpace(t.Category))
            .GroupBy(t => t.Category!)
            .Select(g => new
            {
                Category = g.Key,
                Income = g.Where(t => t.TransactionType == CashTransactionType.Income).Sum(t => t.Amount),
                Expense = g.Where(t => t.TransactionType == CashTransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderByDescending(x => Math.Max(x.Income, x.Expense))
            .Take(5)
            .ToList();

        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var builder = new StringBuilder();

        builder.Append(@"
            <table style='width:100%; border-collapse:collapse;'>
                <tr>
                    <th style='text-align:left; padding:8px; border-bottom:1px solid #e5e7eb;'>Kalem</th>
                    <th style='text-align:right; padding:8px; border-bottom:1px solid #e5e7eb;'>Tutar</th>
                </tr>");
        builder.AppendFormat(culture, "<tr><td style='padding:8px;'>Toplam Gelir</td><td style='padding:8px; text-align:right; color:#16a34a;'>{0:C}</td></tr>", totalIncome);
        builder.AppendFormat(culture, "<tr><td style='padding:8px;'>Toplam Gider</td><td style='padding:8px; text-align:right; color:#dc2626;'>{0:C}</td></tr>", totalExpense);
        builder.AppendFormat(culture, "<tr><td style='padding:8px;'>Net Bakiye</td><td style='padding:8px; text-align:right; font-weight:600;'>{0:C}</td></tr>", net);
        builder.Append("</table>");

        if (topCategories.Count > 0)
        {
            builder.Append("<h3 style='margin-top:24px;'>Kategori Bazlı İlk 5 Kalem</h3><ul>");
            foreach (var category in topCategories)
            {
                var balance = category.Income - category.Expense;
                builder.AppendFormat(culture,
                    "<li><strong>{0}</strong>: Gelir {1:C} | Gider {2:C} | Net {3:C}</li>",
                    category.Category,
                    category.Income,
                    category.Expense,
                    balance);
            }
            builder.Append("</ul>");
        }

        return new Dictionary<string, string>
        {
            ["Title"] = "Finans Özeti",
            ["Description"] = $"{start:dd.MM.yyyy} - {end:dd.MM.yyyy} tarihleri arasındaki finansal sonuçlar",
            ["Content"] = builder.ToString()
        };
    }

    private static IDictionary<string, string> NormalizePlaceholders(IDictionary<string, string> source)
    {
        var placeholders = source != null
            ? new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!placeholders.ContainsKey("Title"))
        {
            placeholders["Title"] = "CRM Bildirimi";
        }

        if (!placeholders.ContainsKey("Description"))
        {
            placeholders["Description"] = string.Empty;
        }

        if (!placeholders.ContainsKey("Content"))
        {
            placeholders["Content"] = string.Empty;
        }

        if (!placeholders.ContainsKey("Footer"))
        {
            placeholders["Footer"] = "Bu e-posta CRM sistemi tarafından otomatik gönderildi.";
        }

        if (!placeholders.ContainsKey("ActionSection"))
        {
            if (placeholders.TryGetValue("ActionUrl", out var url) &&
                !string.IsNullOrWhiteSpace(url))
            {
                var actionText = placeholders.TryGetValue("ActionText", out var text) && !string.IsNullOrWhiteSpace(text)
                    ? text
                    : "Detayları görüntüle";
                placeholders["ActionSection"] = $"<a href=\"{url}\" class=\"action-button\">{actionText}</a>";
            }
            else
            {
                placeholders["ActionSection"] = string.Empty;
            }
        }

        return placeholders;
    }

    private static FinanceSummaryMetadata ParseFinanceSummaryMetadata(string? metadataRaw)
    {
        if (string.IsNullOrWhiteSpace(metadataRaw))
        {
            return new FinanceSummaryMetadata();
        }

        try
        {
            var metadata = JsonSerializer.Deserialize<FinanceSummaryMetadata>(metadataRaw);
            return metadata ?? new FinanceSummaryMetadata();
        }
        catch
        {
            return new FinanceSummaryMetadata();
        }
    }

    private sealed record FinanceSummaryMetadata
    {
        public int RangeDays { get; init; } = 1;
    }

    private async Task<Dictionary<Guid, bool>> LoadPreferenceMapAsync(
        HashSet<Guid> userIds,
        EmailResourceType resourceType,
        CancellationToken cancellationToken)
    {
        var map = new Dictionary<Guid, bool>();
        if (userIds.Count == 0)
        {
            return map;
        }

        var preferences = await _context.NotificationPreferences
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync(cancellationToken);

        var defaultValue = GetPreferenceValue(null, resourceType);

        foreach (var userId in userIds)
        {
            map[userId] = defaultValue;
        }

        foreach (var preference in preferences)
        {
            map[preference.UserId] = GetPreferenceValue(preference, resourceType);
        }

        return map;
    }

    private static bool ShouldSendToUser(Guid userId, IDictionary<Guid, bool> preferenceMap, EmailResourceType resourceType) =>
        preferenceMap.TryGetValue(userId, out var allowed)
            ? allowed
            : GetPreferenceValue(null, resourceType);

    private static bool GetPreferenceValue(NotificationPreferences? preference, EmailResourceType resourceType) =>
        resourceType switch
        {
            EmailResourceType.Shipment => preference?.EmailShipmentUpdates ?? true,
            EmailResourceType.Finance => preference?.EmailPaymentReminders ?? true,
            EmailResourceType.Warehouse => preference?.EmailWarehouseAlerts ?? true,
            EmailResourceType.Customer => preference?.EmailCustomerInteractions ?? false,
            EmailResourceType.Task => preference?.EmailSystemAnnouncements ?? true,
            _ => preference?.EmailSystemAnnouncements ?? true
        };
}

