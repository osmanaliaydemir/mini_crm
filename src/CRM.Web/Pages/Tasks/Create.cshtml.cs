using System.ComponentModel.DataAnnotations;
using CRM.Application.Tasks;
using CRM.Domain.Tasks;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Tasks;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ITaskService _taskService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreateModel(ITaskService taskService, UserManager<ApplicationUser> userManager,
        ILogger<CreateModel> logger, IStringLocalizer<SharedResource> localizer)
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

    public async System.Threading.Tasks.Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadSelectListsAsync(cancellationToken);
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
            var request = new CreateTaskRequest(Task.Title, Task.Description, Task.Status,
                Task.Priority, Task.DueDate, Task.AssignedToUserId, Task.RelatedEntityType, Task.RelatedEntityId);

            await _taskService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = _localizer["Tasks_Create_Success"].ToString();
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            ModelState.AddModelError(string.Empty, _localizer["Tasks_Create_Error"].ToString());
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
        [Display(Name = "Tasks_Field_Title")]
        [Required(ErrorMessage = "Validation_Required")]
        [MaxLength(200, ErrorMessage = "Validation_MaxLength")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Tasks_Field_Description")]
        [MaxLength(1000, ErrorMessage = "Validation_MaxLength")]
        public string? Description { get; set; }

        [Display(Name = "Tasks_Field_Status")]
        [Required(ErrorMessage = "Validation_Required")]
        public Domain.Tasks.TaskStatus Status { get; set; } = Domain.Tasks.TaskStatus.Pending;

        [Display(Name = "Tasks_Field_Priority")]
        [Required(ErrorMessage = "Validation_Required")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

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
    }
}

