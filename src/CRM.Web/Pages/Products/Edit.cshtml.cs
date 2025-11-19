using System.ComponentModel.DataAnnotations;
using CRM.Application.Products;
using CRM.Domain.Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Products;

public class EditModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IProductService productService, ILogger<EditModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [BindProperty]
    public ProductInput Product { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        // StandardVolume bir Measurement (Amount ve Unit içerir)
        // UI'da Length, Width, Height olarak gösteremeyiz, bu yüzden null bırakıyoruz
        // Kullanıcı isterse yeniden girebilir
        Product = new ProductInput
        {
            Id = product.Id,
            Name = product.Name,
            Species = product.Species,
            Grade = product.Grade,
            StandardVolumeLength = null,
            StandardVolumeWidth = null,
            StandardVolumeHeight = null,
            UnitOfMeasure = product.UnitOfMeasure,
            Notes = product.Notes,
            RowVersion = product.RowVersion
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
            Measurement? standardVolume = null;
            if (Product.StandardVolumeLength.HasValue && Product.StandardVolumeWidth.HasValue && Product.StandardVolumeHeight.HasValue)
            {
                var volume = Product.StandardVolumeLength.Value * Product.StandardVolumeWidth.Value * Product.StandardVolumeHeight.Value;
                standardVolume = Measurement.CubicMeters(volume);
            }

            var request = new UpdateLumberVariantRequest(
                Product.Id,
                Product.Name,
                Product.Species,
                Product.Grade,
                standardVolume,
                Product.UnitOfMeasure,
                Product.Notes,
                Product.RowVersion);

            await _productService.UpdateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Ürün başarıyla güncellendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating product: {ProductId}", Product.Id);
            ModelState.AddModelError(string.Empty, "Bu ürün başka bir kullanıcı tarafından güncellenmiş. Lütfen sayfayı yenileyip tekrar deneyin.");
            // Reload the product to get the latest data
            var product = await _productService.GetByIdAsync(Product.Id, cancellationToken);
            if (product != null)
            {
                Product.RowVersion = product.RowVersion;
            }
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", Product.Id);
            ModelState.AddModelError(string.Empty, "Ürün güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    public sealed class ProductInput
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Display(Name = "Ad")]
        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Tür")]
        [MaxLength(100)]
        public string? Species { get; set; }

        [Display(Name = "Kalite")]
        [MaxLength(50)]
        public string? Grade { get; set; }

        [Display(Name = "Standart Hacim - Uzunluk (m)")]
        public decimal? StandardVolumeLength { get; set; }

        [Display(Name = "Standart Hacim - Genişlik (m)")]
        public decimal? StandardVolumeWidth { get; set; }

        [Display(Name = "Standart Hacim - Yükseklik (m)")]
        public decimal? StandardVolumeHeight { get; set; }

        [Display(Name = "Birim")]
        [Required(ErrorMessage = "Birim gereklidir.")]
        [MaxLength(20)]
        public string UnitOfMeasure { get; set; } = "m3";

        [Display(Name = "Notlar")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [HiddenInput]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

