using CRM.Application.Tasks;
using CRM.Application.Common.Pagination;
using CRM.Domain.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Tasks;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITaskService _taskService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(ITaskService taskService, ILogger<IndexModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _taskService = taskService;
        _logger = logger;
        _localizer = localizer;
    }

    public PagedResult<TaskDto> Tasks { get; private set; } = new(Array.Empty<TaskDto>(), 0, 1, 10);
    public Domain.Tasks.TaskStatus? FilterStatus { get; set; }
    public Guid? FilterAssignedTo { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync(Domain.Tasks.TaskStatus? status, Guid? assignedTo, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            FilterStatus = status;
            FilterAssignedTo = assignedTo;
            CurrentPage = page;
            PageSize = pageSize;

            var pagination = PaginationRequest.Create(page, pageSize);
            Tasks = await _taskService.GetAllPagedAsync(pagination, status, assignedTo, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tasks");
            Tasks = new PagedResult<TaskDto>(Array.Empty<TaskDto>(), 0, 1, pageSize);
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _taskService.DeleteAsync(id, cancellationToken);
            TempData["StatusMessage"] = _localizer["Tasks_Delete_Success"].ToString();
            TempData["StatusMessageType"] = "success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task");
            TempData["StatusMessage"] = _localizer["Tasks_Delete_Error"].ToString();
            TempData["StatusMessageType"] = "error";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, Domain.Tasks.TaskStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            await _taskService.UpdateStatusAsync(id, status, cancellationToken);
            TempData["StatusMessage"] = _localizer["Tasks_Status_Updated"].ToString();
            TempData["StatusMessageType"] = "success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status");
            TempData["StatusMessage"] = _localizer["Tasks_Status_Update_Error"].ToString();
            TempData["StatusMessageType"] = "error";
        }

        return RedirectToPage();
    }
}

