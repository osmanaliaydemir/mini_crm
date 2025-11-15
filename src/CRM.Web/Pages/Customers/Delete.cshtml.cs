using CRM.Application.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Customers;

public class DeleteModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(ICustomerService customerService, ILogger<DeleteModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [BindProperty]
    public Guid CustomerId { get; set; }

    public string? CustomerName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
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
        try
        {
            await _customerService.DeleteAsync(CustomerId, cancellationToken);
            
            TempData["StatusMessage"] = "Müşteri başarıyla silindi.";
            TempData["StatusMessageType"] = "success";
            
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Customer not found: {CustomerId}", CustomerId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", CustomerId);
            ModelState.AddModelError(string.Empty, "Müşteri silinirken bir hata oluştu. Lütfen tekrar deneyin.");
            
            // Customer bilgisini tekrar yükle
            var customer = await _customerService.GetByIdAsync(CustomerId, cancellationToken);
            if (customer != null)
            {
                CustomerName = customer.Name;
            }
            
            return Page();
        }
    }
}

