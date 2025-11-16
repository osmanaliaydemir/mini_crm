using System.ComponentModel.DataAnnotations;
using CRM.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Users;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IUserService userService, ILogger<CreateModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [BindProperty]
    public new UserInput User { get; set; } = new();

    public IReadOnlyList<string> AllRoles { get; private set; } = Array.Empty<string>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        AllRoles = await _userService.GetAllRolesAsync(cancellationToken);
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
            var request = new CreateUserRequest(
                User.UserName,
                User.Email,
                User.Password,
                User.FirstName,
                User.LastName,
                User.Locale,
                User.SelectedRoles?.ToList() ?? new List<string>());

            await _userService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Kullanıcı başarıyla oluşturuldu.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            ModelState.AddModelError(string.Empty, "Kullanıcı oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
            AllRoles = await _userService.GetAllRolesAsync(cancellationToken);
            return Page();
        }
    }

    public sealed class UserInput
    {
        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalıdır.")]
        [MaxLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Parola")]
        [Required(ErrorMessage = "Parola gereklidir.")]
        [MinLength(6, ErrorMessage = "Parola en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ad")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "Soyad")]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [Display(Name = "Yerel Ayar")]
        [MaxLength(10)]
        public string? Locale { get; set; }

        [Display(Name = "Roller")]
        public string[]? SelectedRoles { get; set; }
    }
}

