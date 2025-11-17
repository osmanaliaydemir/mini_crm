using CRM.Application.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Tasks;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ITaskService _taskService;
    private readonly ILogger<DeleteModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteModel(ITaskService taskService, ILogger<DeleteModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _taskService = taskService;
        _logger = logger;
        _localizer = localizer;
    }

    [BindProperty]
    public Guid TaskId { get; set; }

    public string? TaskTitle { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        TaskId = task.Id;
        TaskTitle = task.Title;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _taskService.DeleteAsync(TaskId, cancellationToken);

            TempData["StatusMessage"] = _localizer["Tasks_Delete_Success"].ToString();
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task: {TaskId}", TaskId);
            ModelState.AddModelError(string.Empty, _localizer["Tasks_Delete_Error"].ToString());

            var task = await _taskService.GetByIdAsync(TaskId, cancellationToken);
            if (task != null)
            {
                TaskTitle = task.Title;
            }

            return Page();
        }
    }
}

