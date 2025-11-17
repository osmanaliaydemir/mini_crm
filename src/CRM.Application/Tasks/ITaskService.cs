using CRM.Application.Common.Pagination;
using CRM.Domain.Tasks;

namespace CRM.Application.Tasks;

public interface ITaskService
{
    Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskDto>> GetAllAsync(Domain.Tasks.TaskStatus? status = null, Guid? assignedToUserId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskDto>> GetAllPagedAsync(PaginationRequest pagination, Domain.Tasks.TaskStatus? status = null, Guid? assignedToUserId = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, Domain.Tasks.TaskStatus status, CancellationToken cancellationToken = default);
    Task AssignToAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default);
}

