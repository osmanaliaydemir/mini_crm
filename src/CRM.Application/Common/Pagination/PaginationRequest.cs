namespace CRM.Application.Common.Pagination;

/// <summary>
/// Represents a pagination request with page number and page size.
/// </summary>
public class PaginationRequest
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// The page number (1-based). Defaults to 1.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? DefaultPageNumber : value;
    }

    /// <summary>
    /// The number of items per page. Defaults to 10, maximum is 100.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : (value > MaxPageSize ? MaxPageSize : value);
    }

    /// <summary>
    /// Creates a default pagination request (page 1, 10 items per page).
    /// </summary>
    public static PaginationRequest Default => new();

    /// <summary>
    /// Creates a pagination request with the specified page number and page size.
    /// </summary>
    public static PaginationRequest Create(int pageNumber, int pageSize)
    {
        return new PaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

