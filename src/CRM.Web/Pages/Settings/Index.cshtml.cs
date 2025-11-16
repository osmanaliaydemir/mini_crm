using CRM.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace CRM.Web.Pages.Settings;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ISystemSettingsService _settingsService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(
        ISystemSettingsService settingsService,
        ILogger<IndexModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _settingsService = settingsService;
        _logger = logger;
        _localizer = localizer;
    }

    public SystemSettingsDto? Settings { get; private set; }

    [BindProperty]
    public CompanyInfoInput CompanyInfo { get; set; } = new();

    [BindProperty]
    public SmtpSettingsInput SmtpSettings { get; set; } = new();

    [BindProperty]
    public SystemSettingsInput SystemSettings { get; set; } = new();

    [BindProperty]
    public MaintenanceSettingsInput MaintenanceSettings { get; set; } = new();

    public string ActiveTab { get; set; } = "company";

    public async Task<IActionResult> OnGetAsync(string? tab = null, CancellationToken cancellationToken = default)
    {
        ActiveTab = tab ?? "company";

        Settings = await _settingsService.GetSettingsAsync(cancellationToken);

        if (Settings != null)
        {
            CompanyInfo.CompanyName = Settings.CompanyName;
            CompanyInfo.CompanyEmail = Settings.CompanyEmail;
            CompanyInfo.CompanyPhone = Settings.CompanyPhone;
            CompanyInfo.CompanyAddress = Settings.CompanyAddress;
            CompanyInfo.CompanyTaxNumber = Settings.CompanyTaxNumber;
            CompanyInfo.CompanyLogoUrl = Settings.CompanyLogoUrl;

            SmtpSettings.SmtpHost = Settings.SmtpHost;
            SmtpSettings.SmtpPort = Settings.SmtpPort;
            SmtpSettings.SmtpUsername = Settings.SmtpUsername;
            SmtpSettings.SmtpEnableSsl = Settings.SmtpEnableSsl;
            SmtpSettings.SmtpFromEmail = Settings.SmtpFromEmail;
            SmtpSettings.SmtpFromName = Settings.SmtpFromName;

            SystemSettings.SessionTimeoutMinutes = Settings.SessionTimeoutMinutes;
            SystemSettings.EnableEmailNotifications = Settings.EnableEmailNotifications;
            SystemSettings.EnableSmsNotifications = Settings.EnableSmsNotifications;
            SystemSettings.SmsProvider = Settings.SmsProvider;

            MaintenanceSettings.AuditLogRetentionDays = Settings.AuditLogRetentionDays;
            MaintenanceSettings.BackupRetentionDays = Settings.BackupRetentionDays;
            MaintenanceSettings.EnableAutoBackup = Settings.EnableAutoBackup;
            MaintenanceSettings.BackupSchedule = Settings.BackupSchedule;
        }
        else
        {
            // Default values
            SystemSettings.SessionTimeoutMinutes = 60;
            SystemSettings.EnableEmailNotifications = true;
            SystemSettings.EnableSmsNotifications = false;
            MaintenanceSettings.AuditLogRetentionDays = 90;
            MaintenanceSettings.BackupRetentionDays = 30;
            MaintenanceSettings.EnableAutoBackup = false;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCompanyInfoAsync(CancellationToken cancellationToken)
    {
        ActiveTab = "company";

        if (!ModelState.IsValid)
        {
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new UpdateCompanyInfoRequest(
                CompanyInfo.CompanyName,
                CompanyInfo.CompanyEmail,
                CompanyInfo.CompanyPhone,
                CompanyInfo.CompanyAddress,
                CompanyInfo.CompanyTaxNumber,
                CompanyInfo.CompanyLogoUrl);

            await _settingsService.UpdateCompanyInfoAsync(request, cancellationToken);

            TempData["StatusMessage"] = _localizer["Settings_Success_CompanyUpdated"].ToString();
            TempData["StatusMessageType"] = "success";
            return RedirectToPage(new { tab = "company" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company information");
            ModelState.AddModelError(string.Empty, _localizer["Settings_Error_UpdateFailed"]);
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostSmtpSettingsAsync(CancellationToken cancellationToken)
    {
        ActiveTab = "smtp";

        if (!ModelState.IsValid)
        {
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new UpdateSmtpSettingsRequest(
                SmtpSettings.SmtpHost,
                SmtpSettings.SmtpPort,
                SmtpSettings.SmtpUsername,
                SmtpSettings.SmtpPassword,
                SmtpSettings.SmtpEnableSsl,
                SmtpSettings.SmtpFromEmail,
                SmtpSettings.SmtpFromName);

            await _settingsService.UpdateSmtpSettingsAsync(request, cancellationToken);

            TempData["StatusMessage"] = _localizer["Settings_Success_SmtpUpdated"].ToString();
            TempData["StatusMessageType"] = "success";
            return RedirectToPage(new { tab = "smtp" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SMTP settings");
            ModelState.AddModelError(string.Empty, _localizer["Settings_Error_UpdateFailed"]);
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostSystemSettingsAsync(CancellationToken cancellationToken)
    {
        ActiveTab = "system";

        if (!ModelState.IsValid)
        {
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new UpdateSystemSettingsRequest(
                SystemSettings.SessionTimeoutMinutes,
                SystemSettings.EnableEmailNotifications,
                SystemSettings.EnableSmsNotifications,
                SystemSettings.SmsProvider,
                SystemSettings.SmsApiKey);

            await _settingsService.UpdateSystemSettingsAsync(request, cancellationToken);

            TempData["StatusMessage"] = _localizer["Settings_Success_SystemUpdated"].ToString();
            TempData["StatusMessageType"] = "success";
            return RedirectToPage(new { tab = "system" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system settings");
            ModelState.AddModelError(string.Empty, _localizer["Settings_Error_UpdateFailed"]);
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostMaintenanceSettingsAsync(CancellationToken cancellationToken)
    {
        ActiveTab = "maintenance";

        if (!ModelState.IsValid)
        {
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new UpdateMaintenanceSettingsRequest(
                MaintenanceSettings.AuditLogRetentionDays,
                MaintenanceSettings.BackupRetentionDays,
                MaintenanceSettings.EnableAutoBackup,
                MaintenanceSettings.BackupSchedule);

            await _settingsService.UpdateMaintenanceSettingsAsync(request, cancellationToken);

            TempData["StatusMessage"] = _localizer["Settings_Success_MaintenanceUpdated"].ToString();
            TempData["StatusMessageType"] = "success";
            return RedirectToPage(new { tab = "maintenance" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance settings");
            ModelState.AddModelError(string.Empty, _localizer["Settings_Error_UpdateFailed"]);
            Settings = await _settingsService.GetSettingsAsync(cancellationToken);
            return Page();
        }
    }

    public sealed class CompanyInfoInput
    {
        [Display(Name = "Settings_Field_CompanyName")]
        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [Display(Name = "Settings_Field_CompanyEmail")]
        [EmailAddress]
        [MaxLength(200)]
        public string? CompanyEmail { get; set; }

        [Display(Name = "Settings_Field_CompanyPhone")]
        [MaxLength(50)]
        public string? CompanyPhone { get; set; }

        [Display(Name = "Settings_Field_CompanyAddress")]
        [MaxLength(500)]
        public string? CompanyAddress { get; set; }

        [Display(Name = "Settings_Field_CompanyTaxNumber")]
        [MaxLength(50)]
        public string? CompanyTaxNumber { get; set; }

        [Display(Name = "Settings_Field_CompanyLogoUrl")]
        [MaxLength(500)]
        [Url]
        public string? CompanyLogoUrl { get; set; }
    }

    public sealed class SmtpSettingsInput
    {
        [Display(Name = "Settings_Field_SmtpHost")]
        [MaxLength(200)]
        public string? SmtpHost { get; set; }

        [Display(Name = "Settings_Field_SmtpPort")]
        [Range(1, 65535)]
        public int? SmtpPort { get; set; }

        [Display(Name = "Settings_Field_SmtpUsername")]
        [MaxLength(200)]
        public string? SmtpUsername { get; set; }

        [Display(Name = "Settings_Field_SmtpPassword")]
        [MaxLength(500)]
        [DataType(DataType.Password)]
        public string? SmtpPassword { get; set; }

        [Display(Name = "Settings_Field_SmtpEnableSsl")]
        public bool SmtpEnableSsl { get; set; } = true;

        [Display(Name = "Settings_Field_SmtpFromEmail")]
        [EmailAddress]
        [MaxLength(200)]
        public string? SmtpFromEmail { get; set; }

        [Display(Name = "Settings_Field_SmtpFromName")]
        [MaxLength(200)]
        public string? SmtpFromName { get; set; }
    }

    public sealed class SystemSettingsInput
    {
        [Display(Name = "Settings_Field_SessionTimeoutMinutes")]
        [Range(5, 1440)]
        [Required]
        public int SessionTimeoutMinutes { get; set; } = 60;

        [Display(Name = "Settings_Field_EnableEmailNotifications")]
        public bool EnableEmailNotifications { get; set; } = true;

        [Display(Name = "Settings_Field_EnableSmsNotifications")]
        public bool EnableSmsNotifications { get; set; } = false;

        [Display(Name = "Settings_Field_SmsProvider")]
        [MaxLength(100)]
        public string? SmsProvider { get; set; }

        [Display(Name = "Settings_Field_SmsApiKey")]
        [MaxLength(500)]
        [DataType(DataType.Password)]
        public string? SmsApiKey { get; set; }
    }

    public sealed class MaintenanceSettingsInput
    {
        [Display(Name = "Settings_Field_AuditLogRetentionDays")]
        [Range(1, 3650)]
        [Required]
        public int AuditLogRetentionDays { get; set; } = 90;

        [Display(Name = "Settings_Field_BackupRetentionDays")]
        [Range(1, 3650)]
        [Required]
        public int BackupRetentionDays { get; set; } = 30;

        [Display(Name = "Settings_Field_EnableAutoBackup")]
        public bool EnableAutoBackup { get; set; } = false;

        [Display(Name = "Settings_Field_BackupSchedule")]
        [MaxLength(100)]
        public string? BackupSchedule { get; set; }
    }
}

