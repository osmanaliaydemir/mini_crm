using CRM.Application.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Suppliers;

public class DeleteModel : PageModel
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(ISupplierService supplierService, ILogger<DeleteModel> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    [BindProperty]
    public Guid SupplierId { get; set; }

    public string? SupplierName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        SupplierId = supplier.Id;
        SupplierName = supplier.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _supplierService.DeleteAsync(SupplierId, cancellationToken);

            TempData["StatusMessage"] = "Tedarikçi başarıyla silindi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Supplier not found: {SupplierId}", SupplierId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier: {SupplierId}", SupplierId);
            ModelState.AddModelError(string.Empty, "Tedarikçi silinirken bir hata oluştu. Lütfen tekrar deneyin.");

            var supplier = await _supplierService.GetByIdAsync(SupplierId, cancellationToken);
            if (supplier != null)
            {
                SupplierName = supplier.Name;
            }

            return Page();
        }
    }
}

