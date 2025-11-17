using CRM.Application.Timeline;
using CRM.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Timeline;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IActivityTimelineService _timelineService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(
        IActivityTimelineService timelineService,
        ILogger<IndexModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _timelineService = timelineService;
        _logger = logger;
        _localizer = localizer;
    }

    public ActivityTimelineResult? Timeline { get; private set; }

    [BindProperty(SupportsGet = true)]
    public ActivityType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public ActivityAction? Action { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<ActivityTypeOption> ActivityTypeOptions { get; private set; } = Array.Empty<ActivityTypeOption>();
    public IReadOnlyList<ActivityActionOption> ActivityActionOptions { get; private set; } = Array.Empty<ActivityActionOption>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        LoadFilterOptions();

        var filter = new ActivityTimelineFilter(
            Type: Type,
            Action: Action,
            FromDate: FromDate,
            ToDate: ToDate,
            UserId: UserId,
            PageNumber: PageNumber,
            PageSize: 50);

        try
        {
            Timeline = await _timelineService.GetTimelineAsync(filter, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activity timeline");
            Timeline = new ActivityTimelineResult(
                Array.Empty<ActivityTimelineItem>(),
                0,
                PageNumber,
                50,
                0);
        }
    }

    private void LoadFilterOptions()
    {
        ActivityTypeOptions = Enum.GetValues<ActivityType>()
            .Select(t => new ActivityTypeOption(t, _localizer[$"Timeline_ActivityType_{t}"].Value))
            .ToList();

        ActivityActionOptions = Enum.GetValues<ActivityAction>()
            .Select(a => new ActivityActionOption(a, _localizer[$"Timeline_ActivityAction_{a}"].Value))
            .ToList();
    }

    public record ActivityTypeOption(ActivityType Value, string Label);
    public record ActivityActionOption(ActivityAction Value, string Label);
}

