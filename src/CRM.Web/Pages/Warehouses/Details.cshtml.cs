using System.ComponentModel.DataAnnotations;
using CRM.Application.Warehouses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Warehouses;

public class DetailsModel : PageModel
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IWarehouseService warehouseService, ILogger<DetailsModel> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    public WarehouseDetailsDto? Details { get; private set; }

    [BindProperty]
    public UnloadingInputModel UnloadingInput { get; set; } = new();

    public IEnumerable<SelectListItem> ShipmentOptions { get; private set; } = Enumerable.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Details = await _warehouseService.GetDetailsByIdAsync(id, cancellationToken);
        if (Details is null)
        {
            return NotFound();
        }

        ShipmentOptions = Details.ShipmentOptions.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.DisplayText
        });

        UnloadingInput.WarehouseId = id;
        if (UnloadingInput.UnloadedAt == default)
        {
            UnloadingInput.UnloadedAt = DateTime.UtcNow;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Details = await _warehouseService.GetDetailsByIdAsync(UnloadingInput.WarehouseId, cancellationToken);
        if (Details is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ShipmentOptions = Details.ShipmentOptions.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.DisplayText
            });
            return Page();
        }

        try
        {
            var request = new AddUnloadingRequest(UnloadingInput.WarehouseId, UnloadingInput.ShipmentId,
                UnloadingInput.TruckPlate, UnloadingInput.UnloadedAt, UnloadingInput.UnloadedVolume, UnloadingInput.Notes);

            await _warehouseService.AddUnloadingAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Boşaltma kaydı başarıyla eklendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage(new { id = UnloadingInput.WarehouseId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Warehouse not found: {WarehouseId}", UnloadingInput.WarehouseId);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding unloading for warehouse: {WarehouseId}", UnloadingInput.WarehouseId);
            ModelState.AddModelError(string.Empty, "Boşaltma kaydı eklenirken bir hata oluştu. Lütfen tekrar deneyin.");

            ShipmentOptions = Details.ShipmentOptions.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.DisplayText
            });
            return Page();
        }
    }

    public sealed class UnloadingInputModel
    {
        [HiddenInput]
        public Guid WarehouseId { get; set; }

        [Display(Name = "Sevkiyat")]
        [Required]
        public Guid ShipmentId { get; set; }

        [Display(Name = "Tır Plaka")]
        [Required]
        [MaxLength(50)]
        public string TruckPlate { get; set; } = string.Empty;

        [Display(Name = "Boşaltma Tarihi")]
        [Required]
        public DateTime UnloadedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Hacim (m3)")]
        [Range(0.0, double.MaxValue)]
        public decimal UnloadedVolume { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

