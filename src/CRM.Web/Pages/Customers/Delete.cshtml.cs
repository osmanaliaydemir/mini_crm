using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Customers;

public class DeleteModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public DeleteModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public Guid CustomerId { get; set; }

    public string? CustomerName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        CustomerId = customer.Id;
        CustomerName = customer.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == CustomerId, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }
}

