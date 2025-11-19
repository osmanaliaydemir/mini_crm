using CRM.Application.Common;
using CRM.Application.Common.Pagination;

namespace CRM.Application.AuditLogs;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetAllPagedAsync(
        PaginationRequest pagination,
        string? entityType = null,
        Guid? entityId = null,
        string? action = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetAllAsync(
        string? entityType = null,
        Guid? entityId = null,
        string? action = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEntityTypesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Manuel olarak audit log kaydı oluşturur (Login, Logout, FailedLogin gibi işlemler için)
    /// </summary>
    Task CreateLogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default);
}

