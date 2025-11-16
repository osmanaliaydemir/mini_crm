using System.ComponentModel.DataAnnotations;
using CRM.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Users;

[Authorize(Policy = "AdminOnly")]
public class ChangePasswordModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<ChangePasswordModel> _logger;

    public ChangePasswordModel(IUserService userService, ILogger<ChangePasswordModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public Guid UserId { get; set; }
    public string? UserName { get; set; }

    [BindProperty]
    public ChangePasswordInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        UserId = user.Id;
        UserName = user.UserName;
        Input.UserId = user.Id;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var user = await _userService.GetByIdAsync(Input.UserId, cancellationToken);
            if (user != null)
            {
                UserId = user.Id;
                UserName = user.UserName;
            }
            return Page();
        }

        try
        {
            var request = new ChangePasswordRequest(Input.UserId, Input.NewPassword);
            await _userService.ChangePasswordAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Parola başarıyla değiştirildi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Edit", new { id = Input.UserId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", Input.UserId);
            ModelState.AddModelError(string.Empty, "Parola değiştirilirken bir hata oluştu. Lütfen tekrar deneyin.");
            
            var user = await _userService.GetByIdAsync(Input.UserId, cancellationToken);
            if (user != null)
            {
                UserId = user.Id;
                UserName = user.UserName;
            }
            
            return Page();
        }
    }

    public sealed class ChangePasswordInput
    {
        public Guid UserId { get; set; }

        [Display(Name = "Yeni Parola")]
        [Required(ErrorMessage = "Yeni parola gereklidir.")]
        [MinLength(6, ErrorMessage = "Parola en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Display(Name = "Yeni Parola (Tekrar)")]
        [Required(ErrorMessage = "Parola tekrarı gereklidir.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Parolalar eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

