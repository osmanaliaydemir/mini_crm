namespace CRM.Application.Search;

public interface IGlobalSearchService
{
    Task<GlobalSearchResponse> SearchAsync(
        GlobalSearchRequest request,
        CancellationToken cancellationToken = default);
}

