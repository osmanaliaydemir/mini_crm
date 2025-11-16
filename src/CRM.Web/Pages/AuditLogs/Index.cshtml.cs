using CRM.Application.AuditLogs;
using CRM.Application.Common.Pagination;
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

    public IndexModel(
        IAuditLogService auditLogService,
        ILogger<IndexModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _auditLogService = auditLogService;
        _logger = logger;
        _localizer = localizer;
    }

    public PagedResult<AuditLogDto> AuditLogs { get; set; } = default!;
    public IReadOnlyList<string> EntityTypes { get; set; } = Array.Empty<string>();

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

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

    public int TotalPages => AuditLogs?.TotalPages ?? 0;
    public int TotalCount => AuditLogs?.TotalCount ?? 0;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var pagination = PaginationRequest.Create(CurrentPage, PageSize);

            EntityTypes = await _auditLogService.GetEntityTypesAsync(cancellationToken);

            AuditLogs = await _auditLogService.GetAllPagedAsync(
                pagination,
                EntityType,
                null, // entityId - şimdilik null
                Action,
                UserId,
                FromDate,
                ToDate,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit logs");
            AuditLogs = new PagedResult<AuditLogDto>(Array.Empty<AuditLogDto>(), CurrentPage, PageSize, 0);
        }
    }
}

