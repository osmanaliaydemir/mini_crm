using CRM.Application.Common.Pagination;
using CRM.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Users;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IUserService userService, ILogger<IndexModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public string? Search { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IReadOnlyList<UserDto> Users { get; private set; } = Array.Empty<UserDto>();
    public IReadOnlyList<string> AllRoles { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync(string? search, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        try
        {
            Search = search;
            CurrentPage = page ?? 1;
            PageSize = pageSize ?? 10;

            var pagination = PaginationRequest.Create(CurrentPage, PageSize);
            var result = await _userService.GetAllPagedAsync(pagination, search, cancellationToken);

            Users = result.Items;
            TotalCount = result.TotalCount;

            AllRoles = await _userService.GetAllRolesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            Users = Array.Empty<UserDto>();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteAsync(id, cancellationToken);
            TempData["StatusMessage"] = "Kullanıcı başarıyla silindi.";
            TempData["StatusMessageType"] = "success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            TempData["StatusMessage"] = "Kullanıcı silinirken bir hata oluştu.";
            TempData["StatusMessageType"] = "error";
        }

        return RedirectToPage("Index", new { search = Search, page = CurrentPage, pageSize = PageSize });
    }
}

