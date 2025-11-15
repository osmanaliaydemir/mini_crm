using System.ComponentModel.DataAnnotations;
using CRM.Application.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Suppliers;

public class EditModel : PageModel
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<EditModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public EditModel(ISupplierService supplierService, ILogger<EditModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _supplierService = supplierService;
        _logger = logger;
        _localizer = localizer;
    }

    [BindProperty]
    public SupplierInput Supplier { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        Supplier = new SupplierInput
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Country = supplier.Country,
            TaxNumber = supplier.TaxNumber,
            ContactEmail = supplier.ContactEmail,
            ContactPhone = supplier.ContactPhone,
            AddressLine = supplier.AddressLine,
            Notes = supplier.Notes,
            RowVersion = supplier.RowVersion
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new UpdateSupplierRequest(
                Supplier.Id,
                Supplier.Name,
                Supplier.Country,
                Supplier.TaxNumber,
                Supplier.ContactEmail,
                Supplier.ContactPhone,
                Supplier.AddressLine,
                Supplier.Notes,
                Supplier.RowVersion);

            await _supplierService.UpdateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Tedarikçi başarıyla güncellendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating supplier: {SupplierId}", Supplier.Id);
            ModelState.AddModelError(string.Empty, _localizer["Error_ConcurrencyConflict"]);
            // Reload the supplier to get the latest data
            var supplier = await _supplierService.GetByIdAsync(Supplier.Id, cancellationToken);
            if (supplier != null)
            {
                Supplier.RowVersion = supplier.RowVersion;
            }
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Supplier not found: {SupplierId}", Supplier.Id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier: {SupplierId}", Supplier.Id);
            ModelState.AddModelError(string.Empty, "Tedarikçi güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    public sealed class SupplierInput
    {
        [HiddenInput]
        public Guid Id { get; set; }

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

        [HiddenInput]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

