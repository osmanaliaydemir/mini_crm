namespace CRM.Application.Search;

public enum SearchEntityType
{
    Customer,
    Shipment,
    Supplier,
    Warehouse,
    Task,
    PaymentPlan,
    CashTransaction,
    User,
    CustomerInteraction
}

public sealed record GlobalSearchResult(
    SearchEntityType Type,
    Guid Id,
    string Title,
    string? Subtitle,
    string? Description,
    string? Icon,
    string? Url,
    DateTime? CreatedAt,
    string? CreatedBy,
    int RelevanceScore);

public sealed record GlobalSearchResponse(
    IReadOnlyList<GlobalSearchResult> Results,
    int TotalCount,
    Dictionary<SearchEntityType, int> CountByType);

public sealed record GlobalSearchRequest(
    string Query,
    IReadOnlyList<SearchEntityType>? EntityTypes = null,
    int MaxResults = 50);

