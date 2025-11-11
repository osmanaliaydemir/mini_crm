using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Domain.Enums;
using CRM.Domain.Shipments;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Shipments;

public class CreateModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public CreateModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public ShipmentInput Input { get; set; } = new();

    public IList<SelectListItem> SupplierOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> CustomerOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> StatusOptions { get; private set; } = new List<SelectListItem>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Input.StageStartedAt = DateTime.UtcNow;
        Input.ShipmentDate = DateTime.UtcNow.Date;
        await LoadLookupsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var shipmentDate = DateTime.SpecifyKind(Input.ShipmentDate, DateTimeKind.Utc);
        DateTime? estimatedArrival = Input.EstimatedArrival.HasValue
            ? DateTime.SpecifyKind(Input.EstimatedArrival.Value, DateTimeKind.Utc)
            : null;
        var stageStartedAt = DateTime.SpecifyKind(Input.StageStartedAt, DateTimeKind.Utc);
        DateTime? stageCompletedAt = Input.StageCompletedAt.HasValue
            ? DateTime.SpecifyKind(Input.StageCompletedAt.Value, DateTimeKind.Utc)
            : null;

        var shipment = new Shipment(
            Guid.NewGuid(),
            Input.SupplierId,
            Input.ReferenceNumber.Trim(),
            shipmentDate,
            Input.Status,
            Input.CustomerId);

        shipment.Update(
            shipmentDate,
            estimatedArrival,
            Input.Status,
            Input.LoadingPort,
            Input.DischargePort,
            Input.Notes,
            Input.CustomerId);

        shipment.SetOrUpdateStage(Input.Status, stageStartedAt, stageCompletedAt, Input.StageNotes);

        _dbContext.Shipments.Add(shipment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Details", new { id = shipment.Id });
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        SupplierOptions = await _dbContext.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
            .ToListAsync(cancellationToken);

        CustomerOptions = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync(cancellationToken);

        StatusOptions = Enum.GetValues(typeof(ShipmentStatus))
            .Cast<ShipmentStatus>()
            .Select(status => new SelectListItem(status.ToString(), status.ToString()))
            .ToList();
    }

    public sealed class ShipmentInput
    {
        [Required]
        public Guid SupplierId { get; set; }

        public Guid? CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime ShipmentDate { get; set; } = DateTime.UtcNow.Date;

        [DataType(DataType.Date)]
        public DateTime? EstimatedArrival { get; set; }

        [Required]
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Ordered;

        [MaxLength(150)]
        public string? LoadingPort { get; set; }

        [MaxLength(150)]
        public string? DischargePort { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StageStartedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? StageCompletedAt { get; set; }

        [MaxLength(500)]
        public string? StageNotes { get; set; }
    }
}


