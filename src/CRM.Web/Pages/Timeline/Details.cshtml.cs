using CRM.Application.Timeline;
using CRM.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Timeline;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IActivityTimelineService _timelineService;
    private readonly ILogger<DetailsModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DetailsModel(
        IActivityTimelineService timelineService,
        ILogger<DetailsModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _timelineService = timelineService;
        _logger = logger;
        _localizer = localizer;
    }

    public ActivityTimelineResult? Timeline { get; private set; }
    public string? EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string? EntityName { get; private set; }
    public ActivityType ActivityType { get; private set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(int entityType, Guid entityId, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(ActivityType), entityType))
        {
            return NotFound();
        }

        var activityType = (ActivityType)entityType;
        EntityType = activityType.ToString();
        EntityId = entityId;
        ActivityType = activityType;

        var entityTypeName = MapActivityTypeToEntityTypeName(activityType);
        if (string.IsNullOrWhiteSpace(entityTypeName))
        {
            return NotFound();
        }

        try
        {
            var filter = new ActivityTimelineFilter(
                PageNumber: PageNumber,
                PageSize: 50);

            Timeline = await _timelineService.GetEntityTimelineAsync(
                entityTypeName,
                entityId,
                filter,
                cancellationToken);

            if (Timeline.Items.Count > 0)
            {
                EntityName = Timeline.Items.First().EntityName;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading entity timeline. EntityType: {EntityType}, EntityId: {EntityId}", activityType, entityId);
            return NotFound();
        }
    }

    private static string? MapActivityTypeToEntityTypeName(ActivityType type) =>
        type switch
        {
            ActivityType.Shipment => "Shipment",
            ActivityType.Customer => "Customer",
            ActivityType.Task => "TaskDb",
            ActivityType.Finance => "CashTransaction",
            ActivityType.Warehouse => "Warehouse",
            ActivityType.Supplier => "Supplier",
            ActivityType.CustomerInteraction => "CustomerInteraction",
            ActivityType.EmailAutomation => "EmailAutomationRule",
            _ => null
        };
}

