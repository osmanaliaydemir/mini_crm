using System.ComponentModel.DataAnnotations;
using CRM.Application.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Customers;

public class DetailsModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ICustomerService customerService, ILogger<DetailsModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    public CustomerDetailsDto? Details { get; private set; }

    [BindProperty]
    public InteractionInput InteractionForm { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Details = await _customerService.GetDetailsByIdAsync(id, cancellationToken);
        if (Details is null)
        {
            return NotFound();
        }

        InteractionForm.CustomerId = id;
        if (InteractionForm.InteractionDate == default)
        {
            InteractionForm.InteractionDate = DateTime.UtcNow;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Details = await _customerService.GetDetailsByIdAsync(InteractionForm.CustomerId, cancellationToken);
        if (Details is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new AddInteractionRequest(
                InteractionForm.InteractionDate,
                InteractionForm.InteractionType,
                InteractionForm.Subject,
                InteractionForm.Notes,
                InteractionForm.RecordedBy);

            await _customerService.AddInteractionAsync(InteractionForm.CustomerId, request, cancellationToken);

            TempData["StatusMessage"] = "Etkileşim başarıyla eklendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage(new { id = InteractionForm.CustomerId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Customer not found: {CustomerId}", InteractionForm.CustomerId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding interaction for customer: {CustomerId}", InteractionForm.CustomerId);
            ModelState.AddModelError(string.Empty, "Etkileşim eklenirken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    public sealed class InteractionInput
    {
        [HiddenInput]
        public Guid CustomerId { get; set; }

        [Display(Name = "İşlem Tarihi")]
        [Required]
        public DateTime InteractionDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Etkileşim Tipi")]
        [Required]
        [MaxLength(100)]
        public string InteractionType { get; set; } = string.Empty;

        [Display(Name = "Konu")]
        [MaxLength(200)]
        public string? Subject { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Kaydeden")]
        [MaxLength(100)]
        public string? RecordedBy { get; set; }
    }
}

