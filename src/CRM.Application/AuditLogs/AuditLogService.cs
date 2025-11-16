using CRM.Application.Common;
using CRM.Application.Common.Pagination;
using CRM.Domain.Audit;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IApplicationDbContext context,
        ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<AuditLogDto>> GetAllPagedAsync(
        PaginationRequest pagination,
        string? entityType = null,
        Guid? entityId = null,
        string? action = null,
        string? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var auditLogs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var auditLogDtos = auditLogs.Adapt<List<AuditLogDto>>();

        return new PagedResult<AuditLogDto>(
            auditLogDtos,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return auditLog?.Adapt<AuditLogDto>();
    }

    public async Task<IReadOnlyList<string>> GetEntityTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Select(a => a.EntityType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateLogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog(
                request.EntityType,
                request.EntityId,
                request.Action,
                request.UserId,
                request.UserName,
                request.Changes,
                request.IpAddress,
                request.UserAgent);

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for {EntityType} {Action}", request.EntityType, request.Action);
            // Audit log oluşturma hatası uygulama akışını bozmamalı
            // Bu yüzden exception'ı fırlatmıyoruz, sadece logluyoruz
        }
    }
}

