using CRM.Infrastructure.Persistence;
using CRM.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Warehouses;

public class DeleteModel : PageModel
{
    private readonly CRMDbContext _dbContext;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public DeleteModel(CRMDbContext dbContext, IStringLocalizer<SharedResource> localizer)
    {
        _dbContext = dbContext;
        _localizer = localizer;
    }

    [BindProperty]
    public Guid WarehouseId { get; set; }

    public string? WarehouseName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
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
        var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(w => w.Id == WarehouseId, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        _dbContext.Warehouses.Remove(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = _localizer["Warehouses_Delete_Success", warehouse.Name].Value;
        TempData["StatusMessageType"] = "success";

        return RedirectToPage("Index");
    }
}

