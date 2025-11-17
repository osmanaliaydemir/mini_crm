using CRM.Application.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Tasks;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ITaskService _taskService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ITaskService taskService, ILogger<DetailsModel> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    public TaskDto? Task { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (Task is null)
        {
            return NotFound();
        }

        return Page();
    }
}

