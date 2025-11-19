using System.ComponentModel.DataAnnotations;
using CRM.Application.Products;
using CRM.Domain.Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Products;

public class CreateModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IProductService productService, ILogger<CreateModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [BindProperty]
    public ProductInput Product { get; set; } = new();

    public void OnGet()
    {
        Product.UnitOfMeasure = "m3";
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

            var request = new CreateLumberVariantRequest(
                Product.Name,
                Product.Species,
                Product.Grade,
                standardVolume,
                Product.UnitOfMeasure,
                Product.Notes);

            await _productService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Ürün başarıyla oluşturuldu.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError(string.Empty, "Ürün oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    public sealed class ProductInput
    {
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
    }
}

