using CRM.Application.Warehouses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Warehouses;

public class DeleteModel : PageModel
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(IWarehouseService warehouseService, ILogger<DeleteModel> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    [BindProperty]
    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        WarehouseId = warehouse.Id;
        WarehouseName = warehouse.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _warehouseService.DeleteAsync(WarehouseId, cancellationToken);

            TempData["StatusMessage"] = "Depo başarıyla silindi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Warehouse not found: {WarehouseId}", WarehouseId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse: {WarehouseId}", WarehouseId);
            ModelState.AddModelError(string.Empty, "Depo silinirken bir hata oluştu. Lütfen tekrar deneyin.");

            var warehouse = await _warehouseService.GetByIdAsync(WarehouseId, cancellationToken);
            if (warehouse != null)
            {
                WarehouseName = warehouse.Name;
            }

            return Page();
        }
    }
}

