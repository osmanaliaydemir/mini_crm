using CRM.Domain.Suppliers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Suppliers;

public class DetailsModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public DetailsModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Supplier? Supplier { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Supplier = await _dbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (Supplier is null)
        {
            return NotFound();
        }

        return Page();
    }
}

