using System.ComponentModel.DataAnnotations;
using CRM.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Users;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IUserService userService, ILogger<EditModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [BindProperty]
    public new UserInput User { get; set; } = new();

    public IReadOnlyList<string> AllRoles { get; private set; } = Array.Empty<string>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        User.Id = user.Id;
        User.Email = user.Email ?? string.Empty;
        User.FirstName = user.FirstName;
        User.LastName = user.LastName;
        User.Locale = user.Locale;
        User.EmailConfirmed = user.EmailConfirmed;
        User.LockoutEnabled = user.LockoutEnabled;
        User.LockoutEnd = user.LockoutEnd;
        User.SelectedRoles = user.Roles.ToArray();

        AllRoles = await _userService.GetAllRolesAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            AllRoles = await _userService.GetAllRolesAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new UpdateUserRequest(
                User.Id,
                User.Email,
                User.FirstName,
                User.LastName,
                User.Locale,
                User.EmailConfirmed,
                User.LockoutEnabled,
                User.LockoutEnd,
                User.SelectedRoles?.ToList() ?? new List<string>());

            await _userService.UpdateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Kullanıcı başarıyla güncellendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", User.Id);
            ModelState.AddModelError(string.Empty, "Kullanıcı güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
            AllRoles = await _userService.GetAllRolesAsync(cancellationToken);
            return Page();
        }
    }

    public sealed class UserInput
    {
        public Guid Id { get; set; }

        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Ad")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "Soyad")]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [Display(Name = "Yerel Ayar")]
        [MaxLength(10)]
        public string? Locale { get; set; }

        [Display(Name = "E-posta Onaylı")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Hesap Kilidi Etkin")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "Kilit Son Tarih")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "Roller")]
        public string[]? SelectedRoles { get; set; }
    }
}

