namespace CRM.Application.Timeline;

public interface IActivityTimelineService
{
    Task<ActivityTimelineResult> GetTimelineAsync(
        ActivityTimelineFilter filter,
        CancellationToken cancellationToken = default);

    Task<ActivityTimelineResult> GetEntityTimelineAsync(
        string entityType,
        Guid entityId,
        ActivityTimelineFilter? additionalFilter = null,
        CancellationToken cancellationToken = default);
}

