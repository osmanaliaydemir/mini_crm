using CRM.Application.Common;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Tasks;

public class TaskService : ITaskService
{
    private readonly IRepository<TaskDb> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        IRepository<TaskDb> repository,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<TaskService> logger)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null) return null;

        // User names will be loaded in the view layer if needed
        string? assignedToUserName = null;

        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.AssignedToUserId,
            assignedToUserName,
            task.RelatedEntityType,
            task.RelatedEntityId,
            task.CreatedAt,
            task.CreatedBy,
            task.LastModifiedAt,
            task.RowVersion);
    }

    public async Task<IReadOnlyList<TaskDto>> GetAllAsync(Domain.Tasks.TaskStatus? status = null, Guid? assignedToUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (assignedToUserId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        // User names will be loaded in the view layer if needed
        return tasks.Select(t => new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.AssignedToUserId,
            null, // User names will be loaded in the view layer
            t.RelatedEntityType,
            t.RelatedEntityId,
            t.CreatedAt,
            t.CreatedBy,
            t.LastModifiedAt,
            t.RowVersion)).ToList();
    }

    public async Task<PagedResult<TaskDto>> GetAllPagedAsync(PaginationRequest pagination, Domain.Tasks.TaskStatus? status = null, Guid? assignedToUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (assignedToUserId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);
        }

        var pagedResult = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToPagedResultAsync(pagination, cancellationToken);

        // User names will be loaded in the view layer if needed
        var items = pagedResult.Items.Select(t => new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.AssignedToUserId,
            null, // User names will be loaded in the view layer
            t.RelatedEntityType,
            t.RelatedEntityId,
            t.CreatedAt,
            t.CreatedBy,
            t.LastModifiedAt,
            t.RowVersion)).ToList();

        return new PagedResult<TaskDto>(items, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<Guid> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new TaskDb(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate,
            request.AssignedToUserId,
            request.RelatedEntityType,
            request.RelatedEntityId);

        await _repository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.Id;
    }

    public async Task UpdateAsync(UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (task == null)
        {
            throw new NotFoundException(nameof(TaskDb), request.Id);
        }

        task.RowVersion = request.RowVersion;

        task.Update(
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate,
            request.AssignedToUserId,
            request.RelatedEntityType,
            request.RelatedEntityId);

        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task == null)
        {
            throw new NotFoundException(nameof(TaskDb), id);
        }

        await _repository.DeleteAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, Domain.Tasks.TaskStatus status, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task == null)
        {
            throw new NotFoundException(nameof(TaskDb), id);
        }

        task.UpdateStatus(status);
        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignToAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task == null)
        {
            throw new NotFoundException(nameof(TaskDb), id);
        }

        task.AssignTo(userId);
        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

