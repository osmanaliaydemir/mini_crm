using System.ComponentModel.DataAnnotations;
using CRM.Application.Warehouses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Warehouses;

public class EditModel : PageModel
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<EditModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public EditModel(IWarehouseService warehouseService, ILogger<EditModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _warehouseService = warehouseService;
        _logger = logger;
        _localizer = localizer;
    }

    [BindProperty]
    public WarehouseInput Warehouse { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        Warehouse = new WarehouseInput
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Location = warehouse.Location,
            ContactPerson = warehouse.ContactPerson,
            ContactPhone = warehouse.ContactPhone,
            Notes = warehouse.Notes,
            RowVersion = warehouse.RowVersion
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
            var request = new UpdateWarehouseRequest(Warehouse.Id, Warehouse.Name,
                Warehouse.Location, Warehouse.ContactPerson, Warehouse.ContactPhone,
                Warehouse.Notes, Warehouse.RowVersion);

            await _warehouseService.UpdateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Depo başarıyla güncellendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating warehouse: {WarehouseId}", Warehouse.Id);
            ModelState.AddModelError(string.Empty, _localizer["Error_ConcurrencyConflict"]);
            // Reload the warehouse to get the latest data
            var warehouse = await _warehouseService.GetByIdAsync(Warehouse.Id, cancellationToken);
            if (warehouse != null)
            {
                Warehouse.RowVersion = warehouse.RowVersion;
            }
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Warehouse not found: {WarehouseId}", Warehouse.Id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse: {WarehouseId}", Warehouse.Id);
            ModelState.AddModelError(string.Empty, "Depo güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    public sealed class WarehouseInput
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Display(Name = "Depo Adı")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Lokasyon")]
        [MaxLength(200)]
        public string? Location { get; set; }

        [Display(Name = "İrtibat Kişisi")]
        [MaxLength(150)]
        public string? ContactPerson { get; set; }

        [Display(Name = "Telefon")]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [HiddenInput]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

