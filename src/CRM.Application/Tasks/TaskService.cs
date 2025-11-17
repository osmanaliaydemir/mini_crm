using CRM.Application.Common;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Application.Notifications.Automation;
using CRM.Domain.Notifications;
using CRM.Domain.Tasks;
using TaskStatus = CRM.Domain.Tasks.TaskStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Tasks;

public class TaskService : ITaskService
{
    private readonly IRepository<TaskDb> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskService> _logger;
    private readonly IEmailAutomationService _emailAutomationService;
    private readonly IUserDirectory _userDirectory;

    public TaskService(
        IRepository<TaskDb> repository,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<TaskService> logger,
        IEmailAutomationService emailAutomationService,
        IUserDirectory userDirectory)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _emailAutomationService = emailAutomationService;
        _userDirectory = userDirectory;
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

        if (task.AssignedToUserId.HasValue)
        {
            await SendTaskAssignedNotificationAsync(task, task.AssignedToUserId.Value, cancellationToken);
        }

        return task.Id;
    }

    public async Task UpdateAsync(UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (task == null)
        {
            throw new NotFoundException(nameof(TaskDb), request.Id);
        }

        var previousAssignedUserId = task.AssignedToUserId;
        var previousStatus = task.Status;

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

        if (task.AssignedToUserId.HasValue &&
            task.AssignedToUserId != previousAssignedUserId)
        {
            await SendTaskAssignedNotificationAsync(task, task.AssignedToUserId.Value, cancellationToken);
        }

        if (task.Status == TaskStatus.Completed &&
            previousStatus != TaskStatus.Completed)
        {
            await SendTaskCompletedNotificationAsync(task, cancellationToken);
        }
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

        var previousStatus = task.Status;

        task.UpdateStatus(status);
        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (status == TaskStatus.Completed && previousStatus != TaskStatus.Completed)
        {
            await SendTaskCompletedNotificationAsync(task, cancellationToken);
        }
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

        if (userId.HasValue)
        {
            await SendTaskAssignedNotificationAsync(task, userId.Value, cancellationToken);
        }
    }

    private async Task SendTaskAssignedNotificationAsync(TaskDb task, Guid assignedUserId, CancellationToken cancellationToken)
    {
        var context = new EmailAutomationEventContext
        {
            ResourceType = EmailResourceType.Task,
            TriggerType = EmailTriggerType.TaskAssigned,
            RelatedEntityId = task.Id,
            TemplateKey = "GenericNotification",
            Subject = $"Görev Atandı - {task.Title}",
            Placeholders = new Dictionary<string, string>
            {
                ["Title"] = "Yeni Görev Ataması",
                ["Description"] = $"{task.Title} başlıklı görev size atandı.",
                ["Content"] = BuildTaskDetailContent(task),
                ["Footer"] = "Bu e-posta görev ataması gerçekleştiği için gönderildi."
            },
            AdditionalUserIds = new[] { assignedUserId },
            ForceSendWhenNoRule = true
        };

        await _emailAutomationService.HandleEventAsync(context, cancellationToken);
    }

    private async Task SendTaskCompletedNotificationAsync(TaskDb task, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(task.CreatedBy))
        {
            return;
        }

        var creatorEmail = await _userDirectory.GetEmailByUserNameAsync(task.CreatedBy, cancellationToken);
        if (string.IsNullOrWhiteSpace(creatorEmail))
        {
            return;
        }

        var context = new EmailAutomationEventContext
        {
            ResourceType = EmailResourceType.Task,
            TriggerType = EmailTriggerType.TaskCompleted,
            RelatedEntityId = task.Id,
            TemplateKey = "GenericNotification",
            Subject = $"Görev Tamamlandı - {task.Title}",
            Placeholders = new Dictionary<string, string>
            {
                ["Title"] = "Görev Tamamlandı",
                ["Description"] = $"{task.Title} başlıklı görev tamamlandı.",
                ["Content"] = BuildTaskDetailContent(task),
                ["Footer"] = "Bu e-posta görev durumu tamamlandığı için gönderildi."
            },
            AdditionalEmails = new[] { creatorEmail },
            ForceSendWhenNoRule = true
        };

        await _emailAutomationService.HandleEventAsync(context, cancellationToken);
    }

    private static string BuildTaskDetailContent(TaskDb task)
    {
        var dueDate = task.DueDate.HasValue ? task.DueDate.Value.ToString("dd.MM.yyyy") : "-";
        var description = string.IsNullOrWhiteSpace(task.Description) ? "Açıklama girilmedi." : task.Description;

        return $@"<p><strong>Öncelik:</strong> {task.Priority}</p>
                  <p><strong>Durum:</strong> {task.Status}</p>
                  <p><strong>Bitiş Tarihi:</strong> {dueDate}</p>
                  <p><strong>Açıklama:</strong> {description}</p>";
    }
}

