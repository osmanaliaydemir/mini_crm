using CRM.Application.Search;
using CRM.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Search;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IGlobalSearchService _searchService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(
        IGlobalSearchService searchService,
        ILogger<IndexModel> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _searchService = searchService;
        _logger = logger;
        _localizer = localizer;
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<string>? EntityTypes { get; set; }

    public GlobalSearchResponse? SearchResults { get; private set; }
    public bool HasSearched { get; private set; }

    public IReadOnlyList<SearchEntityTypeOption> EntityTypeOptions { get; private set; } = Array.Empty<SearchEntityTypeOption>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        LoadEntityTypeOptions();

        if (string.IsNullOrWhiteSpace(Query))
        {
            HasSearched = false;
            return;
        }

        HasSearched = true;

        try
        {
            var entityTypesList = EntityTypes != null && EntityTypes.Count > 0
                ? EntityTypes.Select(e => Enum.TryParse<SearchEntityType>(e, true, out var type) ? type : (SearchEntityType?)null)
                    .Where(t => t.HasValue)
                    .Select(t => t!.Value)
                    .ToList()
                : new List<SearchEntityType>();

            var request = new GlobalSearchRequest(
                Query ?? "",
                entityTypesList.Count > 0 ? entityTypesList : null,
                MaxResults: 100);

            SearchResults = await _searchService.SearchAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search for query: {Query}", Query);
            SearchResults = new GlobalSearchResponse(
                Array.Empty<GlobalSearchResult>(),
                0,
                new Dictionary<SearchEntityType, int>());
        }
    }

    private void LoadEntityTypeOptions()
    {
        EntityTypeOptions = Enum.GetValues<SearchEntityType>()
            .Select(t => new SearchEntityTypeOption(t, _localizer[$"Search_EntityType_{t}"].Value))
            .ToList();
    }


    public record SearchEntityTypeOption(SearchEntityType Value, string Label);
}

