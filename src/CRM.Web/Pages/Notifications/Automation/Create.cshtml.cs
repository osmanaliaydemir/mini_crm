using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CRM.Application.Common;
using CRM.Application.Notifications.Automation;
using CRM.Domain.Notifications;
using CRM.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Notifications.Automation;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private static readonly Dictionary<EmailResourceType, EmailTriggerType[]> TriggerMap =
        new()
        {
            { EmailResourceType.Shipment, new[] { EmailTriggerType.ShipmentStatusChanged, EmailTriggerType.ShipmentNoteAdded } },
            { EmailResourceType.Finance, new[] { EmailTriggerType.FinanceSummaryScheduled } },
            { EmailResourceType.Task, new[] { EmailTriggerType.TaskAssigned, EmailTriggerType.TaskCompleted } },
            { EmailResourceType.Customer, new[] { EmailTriggerType.CustomerCreated } },
            { EmailResourceType.Warehouse, new[] { EmailTriggerType.WarehouseCreated } }
        };

    private readonly IEmailAutomationService _emailAutomationService;
    private readonly IUserDirectory _userDirectory;
    private readonly ILogger<CreateModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateModel(
        IEmailAutomationService emailAutomationService,
        IUserDirectory userDirectory,
        ILogger<CreateModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _emailAutomationService = emailAutomationService;
        _userDirectory = userDirectory;
        _logger = logger;
        _localizer = localizer;
    }

    public IReadOnlyCollection<TriggerOption> TriggerOptions { get; private set; } = Array.Empty<TriggerOption>();
    public IReadOnlyList<SelectOption> ScheduleFrequencyOptions { get; private set; } = Array.Empty<SelectOption>();
    public IReadOnlyList<SelectOption> UserOptions { get; private set; } = Array.Empty<SelectOption>();
    public IReadOnlyList<SelectOption> RoleOptions { get; private set; } = Array.Empty<SelectOption>();

    [BindProperty]
    public CreateRuleInput Input { get; set; } = new();

    public bool IsEditMode => Input?.Id.HasValue == true;

    public async Task<IActionResult> OnGetAsync(Guid? id, CancellationToken cancellationToken)
    {
        await LoadAuxiliaryAsync(cancellationToken);

        if (id.HasValue)
        {
            if (!await LoadRuleIntoInputAsync(id.Value, cancellationToken))
            {
                TempData["StatusMessage"] = _localizer["EmailAutomation_Error_EditLoadFailed"].Value;
                TempData["StatusMessageType"] = "danger";
                return RedirectToPage("Index");
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadAuxiliaryAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var cronExpression = BuildCronExpression(Input);
        if (Input.ExecutionType == EmailExecutionType.Scheduled && cronExpression == null)
        {
            return Page();
        }

        var recipientRequests = ParseRecipients(Input.CustomEmails);

        if (Input.SelectedUserIds?.Any() == true)
        {
            recipientRequests.AddRange(BuildUserRecipients(Input.SelectedUserIds));
        }

        if (Input.SelectedRoles?.Any() == true)
        {
            recipientRequests.AddRange(BuildRoleRecipients(Input.SelectedRoles));
        }

        if (recipientRequests.Count == 0)
        {
            ModelState.AddModelError("Input.CustomEmails", _localizer["EmailAutomation_Error_RecipientsRequired"]);
            return Page();
        }

        var metadata = BuildMetadata(Input);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Guid? relatedEntityId = null;
        if (!string.IsNullOrWhiteSpace(Input.RelatedEntityId) && Guid.TryParse(Input.RelatedEntityId, out var parsed))
        {
            relatedEntityId = parsed;
        }

        try
        {
            if (Input.Id.HasValue)
            {
                var updateRequest = new UpdateEmailAutomationRuleRequest
                {
                    Id = Input.Id.Value,
                    Name = Input.Name.Trim(),
                    ResourceType = Input.ResourceType,
                    TriggerType = Input.TriggerType,
                    ExecutionType = Input.ExecutionType,
                    TemplateKey = string.IsNullOrWhiteSpace(Input.TemplateKey) ? "GenericNotification" : Input.TemplateKey.Trim(),
                    CronExpression = cronExpression,
                    TimeZoneId = Input.TimeZoneId ?? TimeZoneInfo.Local.Id,
                    RelatedEntityId = relatedEntityId,
                    IsActive = Input.IsActive,
                    Metadata = metadata,
                    Recipients = recipientRequests,
                    RowVersion = DecodeRowVersion(Input.RowVersion)
                };

                await _emailAutomationService.UpdateRuleAsync(updateRequest, cancellationToken);
                TempData["StatusMessage"] = _localizer["EmailAutomation_Message_Updated"].Value;
            }
            else
            {
                var request = new CreateEmailAutomationRuleRequest
                {
                    Name = Input.Name.Trim(),
                    ResourceType = Input.ResourceType,
                    TriggerType = Input.TriggerType,
                    ExecutionType = Input.ExecutionType,
                    TemplateKey = string.IsNullOrWhiteSpace(Input.TemplateKey) ? "GenericNotification" : Input.TemplateKey.Trim(),
                    CronExpression = cronExpression,
                    TimeZoneId = Input.TimeZoneId ?? TimeZoneInfo.Local.Id,
                    RelatedEntityId = relatedEntityId,
                    IsActive = Input.IsActive,
                    Metadata = metadata,
                    Recipients = recipientRequests
                };

                await _emailAutomationService.CreateRuleAsync(request, cancellationToken);
                TempData["StatusMessage"] = _localizer["EmailAutomation_Message_Created"].Value;
            }

            TempData["StatusMessageType"] = "success";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mail automation kuralı kaydedilemedi.");
            ModelState.AddModelError(string.Empty, _localizer[Input.Id.HasValue ? "EmailAutomation_Error_UpdateFailed" : "EmailAutomation_Error_CreateFailed"]);
            return Page();
        }
    }

    private async Task LoadAuxiliaryAsync(CancellationToken cancellationToken)
    {
        TriggerOptions = TriggerMap
            .SelectMany(pair => pair.Value.Select(trigger => new TriggerOption(pair.Key, trigger, DescribeTrigger(trigger))))
            .OrderBy(option => option.ResourceType)
            .ThenBy(option => option.TriggerType)
            .ToList();

        ScheduleFrequencyOptions = Enum.GetValues<ScheduleFrequency>()
            .Select(freq => new SelectOption(freq.ToString(), _localizer[$"EmailAutomation_ScheduleFrequency_{freq}"]))
            .ToList();

        Input ??= new CreateRuleInput();
        Input.TemplateKey ??= "GenericNotification";
        Input.TimeZoneId ??= TimeZoneInfo.Local.Id;
        Input.SelectedUserIds ??= new List<Guid>();
        Input.SelectedRoles ??= new List<string>();
        Input.WeeklyDays ??= new List<DayOfWeek>();

        var users = await _userDirectory.GetAllUsersAsync(cancellationToken);
        UserOptions = users
            .Select(u => new SelectOption(u.Id.ToString(), $"{u.DisplayName} ({u.Email})"))
            .ToList();

        var roles = await _userDirectory.GetAllRolesAsync(cancellationToken);
        RoleOptions = roles
            .Select(role => new SelectOption(role, role))
            .ToList();
    }

    private string? BuildCronExpression(CreateRuleInput input)
    {
        if (input.ExecutionType != EmailExecutionType.Scheduled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(input.ScheduledTime) || !TimeSpan.TryParse(input.ScheduledTime, out var timeOfDay))
        {
            ModelState.AddModelError("Input.ScheduledTime", _localizer["EmailAutomation_Error_TimeRequired"]);
            return null;
        }

        return input.ScheduleFrequency switch
        {
            ScheduleFrequency.Weekly => BuildWeeklyCron(input, timeOfDay),
            ScheduleFrequency.Monthly => BuildMonthlyCron(input, timeOfDay),
            _ => $"0 {timeOfDay.Minutes} {timeOfDay.Hours} * * ?"
        };
    }

    private string? BuildWeeklyCron(CreateRuleInput input, TimeSpan timeOfDay)
    {
        if (input.WeeklyDays == null || input.WeeklyDays.Count == 0)
        {
            ModelState.AddModelError("Input.WeeklyDays", _localizer["EmailAutomation_Error_WeeklyDaysRequired"]);
            return null;
        }

        var dayTokens = input.WeeklyDays
            .Distinct()
            .Select(MapDayOfWeekToCronToken)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToList();

        if (dayTokens.Count == 0)
        {
            ModelState.AddModelError("Input.WeeklyDays", _localizer["EmailAutomation_Error_WeeklyDaysRequired"]);
            return null;
        }

        var joinedDays = string.Join(",", dayTokens);
        return $"0 {timeOfDay.Minutes} {timeOfDay.Hours} ? * {joinedDays}";
    }

    private string? BuildMonthlyCron(CreateRuleInput input, TimeSpan timeOfDay)
    {
        if (!input.MonthlyDay.HasValue || input.MonthlyDay < 1 || input.MonthlyDay > 31)
        {
            ModelState.AddModelError("Input.MonthlyDay", _localizer["EmailAutomation_Error_MonthlyDayRequired"]);
            return null;
        }

        return $"0 {timeOfDay.Minutes} {timeOfDay.Hours} {input.MonthlyDay.Value} * ?";
    }

    private static List<EmailAutomationRecipientRequest> ParseRecipients(string? customEmails)
    {
        var recipients = new List<EmailAutomationRecipientRequest>();
        if (string.IsNullOrWhiteSpace(customEmails))
        {
            return recipients;
        }

        var tokens = customEmails
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(email => email.Trim())
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var email in tokens)
        {
            recipients.Add(new EmailAutomationRecipientRequest
            {
                RecipientType = EmailRecipientType.CustomEmail,
                EmailAddress = email
            });
        }

        return recipients;
    }

    private static IEnumerable<EmailAutomationRecipientRequest> BuildUserRecipients(IEnumerable<Guid> userIds) =>
        userIds.Where(id => id != Guid.Empty)
            .Distinct()
            .Select(id => new EmailAutomationRecipientRequest
            {
                RecipientType = EmailRecipientType.User,
                UserId = id
            });

    private static IEnumerable<EmailAutomationRecipientRequest> BuildRoleRecipients(IEnumerable<string> roles) =>
        roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(role => new EmailAutomationRecipientRequest
            {
                RecipientType = EmailRecipientType.Role,
                RoleName = role
            });

    private string DescribeTrigger(EmailTriggerType trigger) =>
        trigger switch
        {
            EmailTriggerType.ShipmentStatusChanged => _localizer["EmailAutomation_Trigger_ShipmentStatus"],
            EmailTriggerType.ShipmentNoteAdded => _localizer["EmailAutomation_Trigger_ShipmentNote"],
            EmailTriggerType.FinanceSummaryScheduled => _localizer["EmailAutomation_Trigger_FinanceSummary"],
            EmailTriggerType.TaskAssigned => _localizer["EmailAutomation_Trigger_TaskAssigned"],
            EmailTriggerType.TaskCompleted => _localizer["EmailAutomation_Trigger_TaskCompleted"],
            EmailTriggerType.CustomerCreated => _localizer["EmailAutomation_Trigger_CustomerCreated"],
            EmailTriggerType.WarehouseCreated => _localizer["EmailAutomation_Trigger_WarehouseCreated"],
            _ => trigger.ToString()
        };

    private string? BuildMetadata(CreateRuleInput input)
    {
        if (input.ResourceType == EmailResourceType.Finance &&
            input.TriggerType == EmailTriggerType.FinanceSummaryScheduled)
        {
            if (!input.FinanceRangeDays.HasValue)
            {
                ModelState.AddModelError(nameof(Input.FinanceRangeDays), _localizer["EmailAutomation_Error_FinanceRangeRequired"]);
                return null;
            }

            var range = Math.Clamp(input.FinanceRangeDays.Value, 1, 30);
            return JsonSerializer.Serialize(new { rangeDays = range });
        }

        return string.IsNullOrWhiteSpace(input.Metadata) ? null : input.Metadata;
    }

    private static string MapDayOfWeekToCronToken(DayOfWeek day) =>
        day switch
        {
            DayOfWeek.Monday => "MON",
            DayOfWeek.Tuesday => "TUE",
            DayOfWeek.Wednesday => "WED",
            DayOfWeek.Thursday => "THU",
            DayOfWeek.Friday => "FRI",
            DayOfWeek.Saturday => "SAT",
            DayOfWeek.Sunday => "SUN",
            _ => string.Empty
        };

    private async Task<bool> LoadRuleIntoInputAsync(Guid id, CancellationToken cancellationToken)
    {
        var rule = await _emailAutomationService.GetByIdAsync(id, cancellationToken);
        if (rule == null)
        {
            return false;
        }

        Input = new CreateRuleInput
        {
            Id = rule.Id,
            RowVersion = Convert.ToBase64String(rule.RowVersion ?? Array.Empty<byte>()),
            Name = rule.Name,
            ResourceType = rule.ResourceType,
            TriggerType = rule.TriggerType,
            ExecutionType = rule.ExecutionType,
            TemplateKey = rule.TemplateKey,
            TimeZoneId = rule.TimeZoneId,
            RelatedEntityId = rule.RelatedEntityId?.ToString(),
            Metadata = rule.Metadata,
            IsActive = rule.IsActive,
            CustomEmails = string.Join(Environment.NewLine,
                rule.Recipients
                    .Where(r => r.RecipientType == EmailRecipientType.CustomEmail && !string.IsNullOrWhiteSpace(r.EmailAddress))
                    .Select(r => r.EmailAddress!.Trim())),
            SelectedUserIds = rule.Recipients
                .Where(r => r.RecipientType == EmailRecipientType.User && r.UserId.HasValue)
                .Select(r => r.UserId!.Value)
                .Distinct()
                .ToList(),
            SelectedRoles = rule.Recipients
                .Where(r => r.RecipientType == EmailRecipientType.Role && !string.IsNullOrWhiteSpace(r.RoleName))
                .Select(r => r.RoleName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };

        if (rule.ExecutionType == EmailExecutionType.Scheduled)
        {
            ApplyScheduleFromCron(Input, rule.CronExpression);
        }

        if (rule.ResourceType == EmailResourceType.Finance &&
            rule.TriggerType == EmailTriggerType.FinanceSummaryScheduled)
        {
            Input.FinanceRangeDays = ExtractFinanceRangeDays(rule.Metadata) ?? 7;
            Input.Metadata = null;
        }

        return true;
    }

    private void ApplyScheduleFromCron(CreateRuleInput input, string? cronExpression)
    {
        input.ScheduleFrequency = ScheduleFrequency.Daily;
        input.WeeklyDays = new List<DayOfWeek>();
        input.MonthlyDay = null;
        input.ScheduledTime = ExtractTimeFromCron(cronExpression);

        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return;
        }

        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 6)
        {
            return;
        }

        var dayOfMonth = parts[3];
        var dayOfWeek = parts[5];

        if (!string.IsNullOrWhiteSpace(dayOfWeek) && dayOfWeek is not "*" and not "?")
        {
            input.ScheduleFrequency = ScheduleFrequency.Weekly;
            input.WeeklyDays = dayOfWeek
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(MapCronTokenToDayOfWeek)
                .Where(day => day.HasValue)
                .Select(day => day!.Value)
                .Distinct()
                .ToList();
        }
        else if (!string.IsNullOrWhiteSpace(dayOfMonth) && dayOfMonth is not "*" and not "?")
        {
            input.ScheduleFrequency = ScheduleFrequency.Monthly;
            if (int.TryParse(dayOfMonth, out var dom))
            {
                input.MonthlyDay = dom;
            }
        }
    }

    private static string? ExtractTimeFromCron(string? cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return null;
        }

        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return null;
        }

        if (int.TryParse(parts[2], out var hour) && int.TryParse(parts[1], out var minute))
        {
            return new TimeSpan(hour, minute, 0).ToString(@"hh\:mm");
        }

        return null;
    }

    private static DayOfWeek? MapCronTokenToDayOfWeek(string token) =>
        token.ToUpperInvariant() switch
        {
            "MON" => DayOfWeek.Monday,
            "TUE" => DayOfWeek.Tuesday,
            "WED" => DayOfWeek.Wednesday,
            "THU" => DayOfWeek.Thursday,
            "FRI" => DayOfWeek.Friday,
            "SAT" => DayOfWeek.Saturday,
            "SUN" => DayOfWeek.Sunday,
            _ => null
        };

    private static byte[] DecodeRowVersion(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Array.Empty<byte>() : Convert.FromBase64String(value);

    private static int? ExtractFinanceRangeDays(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(metadata);
            if (doc.RootElement.TryGetProperty("rangeDays", out var prop) && prop.TryGetInt32(out var value))
            {
                return value;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    public record TriggerOption(EmailResourceType ResourceType, EmailTriggerType TriggerType, string Label);

    public record SelectOption(string Value, string Label);

    public class CreateRuleInput
    {
        public Guid? Id { get; set; }

        public string? RowVersion { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public EmailResourceType ResourceType { get; set; } = EmailResourceType.Shipment;

        [Required]
        public EmailTriggerType TriggerType { get; set; } = EmailTriggerType.ShipmentStatusChanged;

        [Required]
        public EmailExecutionType ExecutionType { get; set; } = EmailExecutionType.EventBased;

        [MaxLength(128)]
        public string? TemplateKey { get; set; } = "GenericNotification";

        public string? ScheduledTime { get; set; }

        public string? TimeZoneId { get; set; }

        public ScheduleFrequency ScheduleFrequency { get; set; } = ScheduleFrequency.Daily;

        public List<DayOfWeek> WeeklyDays { get; set; } = new();

        [Range(1, 31)]
        public int? MonthlyDay { get; set; }

        public bool IsActive { get; set; } = true;

        public string? RelatedEntityId { get; set; }

        public string? Metadata { get; set; }

        [Display(Name = "Alıcı E-postaları")]
        public string? CustomEmails { get; set; }

        [Range(1, 30)]
        public int? FinanceRangeDays { get; set; } = 7;

        public List<Guid> SelectedUserIds { get; set; } = new();

        public List<string> SelectedRoles { get; set; } = new();
    }

    public enum ScheduleFrequency
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }
}

