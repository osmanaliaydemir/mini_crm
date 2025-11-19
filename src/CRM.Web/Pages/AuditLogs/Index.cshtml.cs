using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Application.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.AuditLogs;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(IAuditLogService auditLogService, ILogger<IndexModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _auditLogService = auditLogService;
        _logger = logger;
        _localizer = localizer;
    }

    public IReadOnlyList<AuditLogDto> AuditLogs { get; private set; } = Array.Empty<AuditLogDto>();
    public IReadOnlyList<string> EntityTypes { get; set; } = Array.Empty<string>();
    public string AuditLogsTableJson { get; private set; } = "[]";

    [BindProperty(SupportsGet = true)]
    public string? EntityType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Action { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public int TotalCount => AuditLogs.Count;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            EntityTypes = await _auditLogService.GetEntityTypesAsync(cancellationToken);

            AuditLogs = await _auditLogService.GetAllAsync(
                EntityType,
                null,
                Action,
                UserId,
                FromDate,
                ToDate,
                cancellationToken);

            var tableRows = AuditLogs.Select(log =>
            {
                var actionKey = $"AuditLogs_Action_{log.Action}";
                var actionDisplay = _localizer[actionKey].Value;
                if (string.Equals(actionDisplay, actionKey, StringComparison.Ordinal))
                {
                    actionDisplay = log.Action;
                }

                return new AuditLogTableRow(
                    Timestamp: log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    EntityType: log.EntityType ?? "-",
                    EntityIdShort: $"{log.EntityId:N}"[..8] + "...",
                    ActionDisplay: actionDisplay ?? log.Action ?? "-",
                    ActionCssClass: (log.Action ?? "unknown").ToLowerInvariant(),
                    UserLabel: log.UserName ?? log.UserId ?? "-",
                    IpAddress: log.IpAddress ?? "-",
                    DetailsUrl: Url.Page("Details", new { id = log.Id }) ?? "#"
                );
            }).ToList();

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            AuditLogsTableJson = JsonSerializer.Serialize(tableRows, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit logs");
            AuditLogs = Array.Empty<AuditLogDto>();
            AuditLogsTableJson = "[]";
        }
    }

    private sealed record AuditLogTableRow(
        string Timestamp,
        string EntityType,
        string EntityIdShort,
        string ActionDisplay,
        string ActionCssClass,
        string UserLabel,
        string IpAddress,
        string DetailsUrl);
}

