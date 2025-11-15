using CRM.Application.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Suppliers;

public class DetailsModel : PageModel
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ISupplierService supplierService, ILogger<DetailsModel> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    public SupplierDto? Supplier { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Supplier = await _supplierService.GetByIdAsync(id, cancellationToken);
        if (Supplier is null)
        {
            return NotFound();
        }

        return Page();
    }
}

