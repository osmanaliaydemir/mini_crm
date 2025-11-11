using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Suppliers;

public class DeleteModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public DeleteModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public Guid SupplierId { get; set; }

    public string? SupplierName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == SupplierId, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        _dbContext.Suppliers.Remove(supplier);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }
}

