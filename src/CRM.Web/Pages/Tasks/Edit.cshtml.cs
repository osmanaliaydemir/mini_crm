using System.ComponentModel.DataAnnotations;
using CRM.Application.Tasks;
using CRM.Domain.Tasks;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Tasks;

[Authorize]
public class EditModel : PageModel
{
    private readonly ITaskService _taskService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EditModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public EditModel(ITaskService taskService, UserManager<ApplicationUser> userManager,
        ILogger<EditModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _taskService = taskService;
        _userManager = userManager;
        _logger = logger;
        _localizer = localizer;
    }

    [BindProperty]
    public TaskInput Task { get; set; } = new();

    public List<SelectListItem> Users { get; set; } = new();
    public List<SelectListItem> Statuses { get; set; } = new();
    public List<SelectListItem> Priorities { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        Task = new TaskInput
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            AssignedToUserId = task.AssignedToUserId,
            RelatedEntityType = task.RelatedEntityType,
            RelatedEntityId = task.RelatedEntityId,
            RowVersion = task.RowVersion
        };

        await LoadSelectListsAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new UpdateTaskRequest(Task.Id, Task.Title, Task.Description,
                Task.Status, Task.Priority, Task.DueDate, Task.AssignedToUserId,
                Task.RelatedEntityType, Task.RelatedEntityId, Task.RowVersion);

            await _taskService.UpdateAsync(request, cancellationToken);

            TempData["StatusMessage"] = _localizer["Tasks_Edit_Success"].ToString();
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating task: {TaskId}", Task.Id);
            ModelState.AddModelError(string.Empty, _localizer["Error_ConcurrencyConflict"].ToString());
            var task = await _taskService.GetByIdAsync(Task.Id, cancellationToken);
            if (task != null)
            {
                Task.RowVersion = task.RowVersion;
            }
            await LoadSelectListsAsync(cancellationToken);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task: {TaskId}", Task.Id);
            ModelState.AddModelError(string.Empty, _localizer["Tasks_Edit_Error"].ToString());
            await LoadSelectListsAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadSelectListsAsync(CancellationToken cancellationToken)
    {
        var users = _userManager.Users.OrderBy(u => u.UserName).ToList();
        Users = users.Select(u => new SelectListItem
        {
            Value = u.Id.ToString(),
            Text = u.UserName ?? u.Email ?? string.Empty
        }).ToList();
        Users.Insert(0, new SelectListItem { Value = "", Text = _localizer["Tasks_Field_Unassigned"].Value });

        Statuses = Enum.GetValues<Domain.Tasks.TaskStatus>()
            .Select(s => new SelectListItem
            {
                Value = ((int)s).ToString(),
                Text = _localizer[$"Tasks_Status_{s}"].Value
            }).ToList();

        Priorities = Enum.GetValues<TaskPriority>()
            .Select(p => new SelectListItem
            {
                Value = ((int)p).ToString(),
                Text = _localizer[$"Tasks_Priority_{p}"].Value
            }).ToList();
    }

    public sealed class TaskInput
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Display(Name = "Tasks_Field_Title")]
        [Required(ErrorMessage = "Validation_Required")]
        [MaxLength(200, ErrorMessage = "Validation_MaxLength")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Tasks_Field_Description")]
        [MaxLength(1000, ErrorMessage = "Validation_MaxLength")]
        public string? Description { get; set; }

        [Display(Name = "Tasks_Field_Status")]
        [Required(ErrorMessage = "Validation_Required")]
        public Domain.Tasks.TaskStatus Status { get; set; }

        [Display(Name = "Tasks_Field_Priority")]
        [Required(ErrorMessage = "Validation_Required")]
        public TaskPriority Priority { get; set; }

        [Display(Name = "Tasks_Field_DueDate")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Tasks_Field_AssignedTo")]
        public Guid? AssignedToUserId { get; set; }

        [Display(Name = "Tasks_Field_RelatedEntityType")]
        [MaxLength(100, ErrorMessage = "Validation_MaxLength")]
        public string? RelatedEntityType { get; set; }

        [Display(Name = "Tasks_Field_RelatedEntityId")]
        public Guid? RelatedEntityId { get; set; }

        [HiddenInput]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

