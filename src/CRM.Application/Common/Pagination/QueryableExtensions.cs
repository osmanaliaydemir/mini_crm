using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Common.Pagination;

/// <summary>
/// Extension methods for IQueryable to support pagination.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination to a query and returns a PagedResult.
    /// </summary>
    /// <typeparam name="T">The type of items in the query.</typeparam>
    /// <param name="query">The query to paginate.</param>
    /// <param name="request">The pagination request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A PagedResult containing the paginated items and metadata.</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, request.PageNumber, request.PageSize);
    }

    /// <summary>
    /// Applies pagination to a query and returns a PagedResult.
    /// </summary>
    /// <typeparam name="T">The type of items in the query.</typeparam>
    /// <param name="query">The query to paginate.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A PagedResult containing the paginated items and metadata.</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var request = PaginationRequest.Create(pageNumber, pageSize);
        return await query.ToPagedResultAsync(request, cancellationToken);
    }
}

