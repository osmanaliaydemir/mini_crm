using CRM.Application.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Products;

public class DeleteModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(IProductService productService, ILogger<DeleteModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [BindProperty]
    public Guid ProductId { get; set; }

    public string? ProductName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        ProductId = product.Id;
        ProductName = product.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(ProductId, cancellationToken);

            TempData["StatusMessage"] = "Ürün başarıyla silindi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Product not found or cannot be deleted: {ProductId}", ProductId);
            ModelState.AddModelError(string.Empty, ex.Message);
            
            var product = await _productService.GetByIdAsync(ProductId, cancellationToken);
            if (product != null)
            {
                ProductName = product.Name;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", ProductId);
            ModelState.AddModelError(string.Empty, "Ürün silinirken bir hata oluştu. Lütfen tekrar deneyin.");

            var product = await _productService.GetByIdAsync(ProductId, cancellationToken);
            if (product != null)
            {
                ProductName = product.Name;
            }

            return Page();
        }
    }
}

