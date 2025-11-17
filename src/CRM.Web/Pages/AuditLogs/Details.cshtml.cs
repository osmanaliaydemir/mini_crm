using CRM.Application.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.AuditLogs;

[Authorize(Policy = "AdminOnly")]
public class DetailsModel : PageModel
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<DetailsModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DetailsModel(IAuditLogService auditLogService, ILogger<DetailsModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _auditLogService = auditLogService;
        _logger = logger;
        _localizer = localizer;
    }

    public AuditLogDto? AuditLog { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            AuditLog = await _auditLogService.GetByIdAsync(id, cancellationToken);

            if (AuditLog == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit log details");
            return NotFound();
        }
    }
}

