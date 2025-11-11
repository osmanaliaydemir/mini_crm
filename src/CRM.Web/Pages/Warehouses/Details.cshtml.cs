using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Persistence;
using CRM.Domain.Warehouses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Warehouses;

public class DetailsModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public DetailsModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Warehouse? Warehouse { get; private set; }
    public IList<WarehouseUnloading> Unloadings { get; private set; } = new List<WarehouseUnloading>();

    [BindProperty]
    public UnloadingInputModel UnloadingInput { get; set; } = new();

    public IEnumerable<SelectListItem> ShipmentOptions { get; private set; } = Enumerable.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        await LoadDataAsync(id, cancellationToken);
        if (Warehouse is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadDataAsync(UnloadingInput.WarehouseId, cancellationToken);
        if (Warehouse is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var unloading = new WarehouseUnloading(
            UnloadingInput.WarehouseId,
            UnloadingInput.ShipmentId,
            UnloadingInput.TruckPlate,
            UnloadingInput.UnloadedAt,
            UnloadingInput.UnloadedVolume);

        unloading.Update(UnloadingInput.UnloadedAt, UnloadingInput.UnloadedVolume, UnloadingInput.Notes);

        _dbContext.WarehouseUnloadings.Add(unloading);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage(new { id = UnloadingInput.WarehouseId });
    }

    private async Task LoadDataAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        Warehouse = await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == warehouseId, cancellationToken);
        Unloadings = await _dbContext.WarehouseUnloadings.AsNoTracking()
            .Where(u => u.WarehouseId == warehouseId)
            .OrderByDescending(u => u.UnloadedAt)
            .ToListAsync(cancellationToken);

        ShipmentOptions = await _dbContext.Shipments.AsNoTracking()
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.ReferenceNumber} - {s.Status}" })
            .ToListAsync(cancellationToken);

        UnloadingInput.WarehouseId = warehouseId;
        if (UnloadingInput.UnloadedAt == default)
        {
            UnloadingInput.UnloadedAt = DateTime.UtcNow;
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

