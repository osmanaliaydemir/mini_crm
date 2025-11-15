using System.ComponentModel.DataAnnotations;
using CRM.Application.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Suppliers;

public class CreateModel : PageModel
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ISupplierService supplierService, ILogger<CreateModel> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    [BindProperty]
    public SupplierInput Supplier { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new CreateSupplierRequest(
                Supplier.Name,
                Supplier.Country,
                Supplier.TaxNumber,
                Supplier.ContactEmail,
                Supplier.ContactPhone,
                Supplier.AddressLine,
                Supplier.Notes);

            await _supplierService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Tedarikçi başarıyla oluşturuldu.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            ModelState.AddModelError(string.Empty, "Tedarikçi oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    public sealed class SupplierInput
    {
        [Display(Name = "Ad")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Ülke")]
        [MaxLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Vergi No")]
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress]
        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        [Display(Name = "Telefon")]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [Display(Name = "Adres")]
        [MaxLength(300)]
        public string? AddressLine { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

