using System.Text.Json;
using CRM.Application.Common;
using CRM.Application.Timeline;
using CRM.Domain.Audit;
using CRM.Domain.Customers;
using CRM.Domain.Finance;
using CRM.Domain.Notifications;
using CRM.Domain.Shipments;
using CRM.Domain.Suppliers;
using CRM.Domain.Tasks;
using CRM.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Timeline;

public class ActivityTimelineService : IActivityTimelineService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ActivityTimelineService> _logger;

    public ActivityTimelineService(
        IApplicationDbContext context,
        ILogger<ActivityTimelineService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActivityTimelineResult> GetTimelineAsync(
        ActivityTimelineFilter filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            // Apply filters
            if (filter.Type.HasValue)
            {
                var entityType = MapActivityTypeToEntityType(filter.Type.Value);
                if (!string.IsNullOrWhiteSpace(entityType))
                {
                    query = query.Where(a => a.EntityType == entityType);
                }
            }

            if (filter.Action.HasValue)
            {
                var action = MapActivityActionToAuditAction(filter.Action.Value);
                if (!string.IsNullOrWhiteSpace(action))
                {
                    query = query.Where(a => a.Action == action);
                }
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= filter.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.UserId))
            {
                query = query.Where(a => a.UserId == filter.UserId);
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityId) && Guid.TryParse(filter.EntityId, out var entityId))
            {
                query = query.Where(a => a.EntityId == entityId);
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                query = query.Where(a => a.EntityType == filter.EntityType);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var items = new List<ActivityTimelineItem>();

            foreach (var auditLog in auditLogs)
            {
                var activityType = MapEntityTypeToActivityType(auditLog.EntityType);
                var activityAction = MapAuditActionToActivityAction(auditLog.Action);
                var entityInfo = await GetEntityInfoAsync(auditLog.EntityType, auditLog.EntityId, cancellationToken);

                var metadata = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(auditLog.Changes))
                {
                    try
                    {
                        var changes = JsonSerializer.Deserialize<Dictionary<string, object>>(auditLog.Changes);
                        if (changes != null)
                        {
                            metadata = changes;
                        }
                    }
                    catch
                    {
                        // Ignore JSON parse errors
                    }
                }

                items.Add(new ActivityTimelineItem(
                    auditLog.Id,
                    activityType,
                    activityAction,
                    auditLog.Timestamp,
                    auditLog.EntityId.ToString(),
                    entityInfo.Name,
                    entityInfo.Reference,
                    BuildDescription(auditLog, entityInfo),
                    auditLog.UserId,
                    auditLog.UserName,
                    null,
                    null,
                    metadata.Count > 0 ? metadata : null));
            }

            return new ActivityTimelineResult(
                items,
                totalCount,
                filter.PageNumber,
                filter.PageSize,
                totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activity timeline");
            return new ActivityTimelineResult(
                Array.Empty<ActivityTimelineItem>(),
                0,
                filter.PageNumber,
                filter.PageSize,
                0);
        }
    }

    public async Task<ActivityTimelineResult> GetEntityTimelineAsync(
        string entityType,
        Guid entityId,
        ActivityTimelineFilter? additionalFilter = null,
        CancellationToken cancellationToken = default)
    {
        var filter = additionalFilter ?? new ActivityTimelineFilter();
        filter = filter with { EntityType = entityType, EntityId = entityId.ToString() };

        return await GetTimelineAsync(filter, cancellationToken);
    }

    private static string? MapActivityTypeToEntityType(ActivityType type) =>
        type switch
        {
            ActivityType.Shipment => nameof(Shipment),
            ActivityType.Customer => nameof(Customer),
            ActivityType.Task => nameof(TaskDb),
            ActivityType.Finance => nameof(CashTransaction),
            ActivityType.Warehouse => nameof(Warehouse),
            ActivityType.Supplier => nameof(Supplier),
            ActivityType.CustomerInteraction => nameof(CustomerInteraction),
            ActivityType.EmailAutomation => nameof(EmailAutomationRule),
            _ => null
        };

    private static ActivityType MapEntityTypeToActivityType(string entityType) =>
        entityType switch
        {
            nameof(Shipment) => ActivityType.Shipment,
            nameof(Customer) => ActivityType.Customer,
            nameof(TaskDb) => ActivityType.Task,
            nameof(CashTransaction) => ActivityType.Finance,
            nameof(Warehouse) => ActivityType.Warehouse,
            nameof(Supplier) => ActivityType.Supplier,
            nameof(CustomerInteraction) => ActivityType.CustomerInteraction,
            nameof(EmailAutomationRule) => ActivityType.EmailAutomation,
            _ => ActivityType.Other
        };

    private static string? MapActivityActionToAuditAction(ActivityAction action) =>
        action switch
        {
            ActivityAction.Created => "Created",
            ActivityAction.Updated => "Updated",
            ActivityAction.Deleted => "Deleted",
            _ => null
        };

    private static ActivityAction MapAuditActionToActivityAction(string action) =>
        action switch
        {
            "Created" => ActivityAction.Created,
            "Updated" => ActivityAction.Updated,
            "Deleted" => ActivityAction.Deleted,
            _ => ActivityAction.Updated
        };

    private async Task<(string? Name, string? Reference)> GetEntityInfoAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            return entityType switch
            {
                nameof(Shipment) => await GetShipmentInfoAsync(entityId, cancellationToken),
                nameof(Customer) => await GetCustomerInfoAsync(entityId, cancellationToken),
                nameof(TaskDb) => await GetTaskInfoAsync(entityId, cancellationToken),
                nameof(CashTransaction) => await GetCashTransactionInfoAsync(entityId, cancellationToken),
                nameof(Warehouse) => await GetWarehouseInfoAsync(entityId, cancellationToken),
                nameof(Supplier) => await GetSupplierInfoAsync(entityId, cancellationToken),
                nameof(CustomerInteraction) => await GetCustomerInteractionInfoAsync(entityId, cancellationToken),
                nameof(EmailAutomationRule) => await GetEmailAutomationRuleInfoAsync(entityId, cancellationToken),
                _ => (null, null)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading entity info for {EntityType} {EntityId}", entityType, entityId);
            return (null, null);
        }
    }

    private async Task<(string? Name, string? Reference)> GetShipmentInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .AsNoTracking()
            .Where(s => s.Id == entityId)
            .Select(s => new { s.ReferenceNumber, s.Status, CustomerName = s.Customer != null ? s.Customer.Name : null })
            .FirstOrDefaultAsync(cancellationToken);

        if (shipment == null)
        {
            return (null, null);
        }

        var customerInfo = !string.IsNullOrWhiteSpace(shipment.CustomerName) ? $" - {shipment.CustomerName}" : "";
        return ($"{shipment.ReferenceNumber}{customerInfo} ({shipment.Status})", shipment.ReferenceNumber);
    }

    private async Task<(string? Name, string? Reference)> GetCustomerInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == entityId)
            .Select(c => new { c.Name, c.TaxNumber })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            return (null, null);
        }

        return (customer.Name, customer.TaxNumber);
    }

    private async Task<(string? Name, string? Reference)> GetTaskInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Id == entityId)
            .Select(t => new { t.Title, t.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (task == null)
        {
            return (null, null);
        }

        return (task.Title, task.Status.ToString());
    }

    private async Task<(string? Name, string? Reference)> GetCashTransactionInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var transaction = await _context.CashTransactions
            .AsNoTracking()
            .Where(t => t.Id == entityId)
            .Select(t => new { t.Description, t.Amount, t.TransactionType })
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction == null)
        {
            return (null, null);
        }

        return ($"{transaction.Description} - {transaction.Amount:C}", transaction.TransactionType.ToString());
    }

    private async Task<(string? Name, string? Reference)> GetWarehouseInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.Id == entityId)
            .Select(w => new { w.Name, w.Location })
            .FirstOrDefaultAsync(cancellationToken);

        if (warehouse == null)
        {
            return (null, null);
        }

        return (warehouse.Name, warehouse.Location);
    }

    private async Task<(string? Name, string? Reference)> GetSupplierInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == entityId)
            .Select(s => new { s.Name, s.Country })
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier == null)
        {
            return (null, null);
        }

        return (supplier.Name, supplier.Country);
    }

    private async Task<(string? Name, string? Reference)> GetCustomerInteractionInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var interaction = await _context.CustomerInteractions
            .AsNoTracking()
            .Where(i => i.Id == entityId)
            .Select(i => new { i.Subject, i.InteractionType, CustomerName = i.Customer != null ? i.Customer.Name : null })
            .FirstOrDefaultAsync(cancellationToken);

        if (interaction == null)
        {
            return (null, null);
        }

        return ($"{interaction.InteractionType} - {interaction.Subject ?? "N/A"}", interaction.CustomerName);
    }

    private async Task<(string? Name, string? Reference)> GetEmailAutomationRuleInfoAsync(
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var rule = await _context.EmailAutomationRules
            .AsNoTracking()
            .Where(r => r.Id == entityId)
            .Select(r => new { r.Name, r.ResourceType })
            .FirstOrDefaultAsync(cancellationToken);

        if (rule == null)
        {
            return (null, null);
        }

        return (rule.Name, rule.ResourceType.ToString());
    }

    private static string BuildDescription(AuditLog auditLog, (string? Name, string? Reference) entityInfo)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(entityInfo.Name))
        {
            parts.Add(entityInfo.Name);
        }

        if (!string.IsNullOrWhiteSpace(auditLog.UserName))
        {
            parts.Add($"by {auditLog.UserName}");
        }

        return string.Join(" · ", parts);
    }
}

