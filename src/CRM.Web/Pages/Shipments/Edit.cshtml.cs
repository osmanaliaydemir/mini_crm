using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Shipments;

public class EditModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public EditModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public ShipmentInput Input { get; set; } = new();

    public IList<SelectListItem> SupplierOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> CustomerOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> StatusOptions { get; private set; } = new List<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);

        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment is null)
        {
            return NotFound();
        }

        var latestStage = shipment.Stages
            .OrderByDescending(stage => stage.StartedAt)
            .FirstOrDefault();

        Input = new ShipmentInput
        {
            Id = shipment.Id,
            SupplierId = shipment.SupplierId,
            CustomerId = shipment.CustomerId,
            ReferenceNumber = shipment.ReferenceNumber,
            ShipmentDate = ToLocal(shipment.ShipmentDate),
            EstimatedArrival = shipment.EstimatedArrival.HasValue ? ToLocal(shipment.EstimatedArrival.Value) : null,
            Status = shipment.Status,
            LoadingPort = shipment.LoadingPort,
            DischargePort = shipment.DischargePort,
            Notes = shipment.Notes,
            StageStartedAt = latestStage is null ? DateTime.UtcNow : ToLocal(latestStage.StartedAt),
            StageCompletedAt = latestStage?.CompletedAt.HasValue == true ? ToLocal(latestStage!.CompletedAt!.Value) : null,
            StageNotes = latestStage?.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var shipment = await _dbContext.Shipments
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == Input.Id, cancellationToken);

        if (shipment is null)
        {
            return NotFound();
        }

        shipment.ReassignSupplier(Input.SupplierId);
        shipment.Update(
            DateTime.SpecifyKind(Input.ShipmentDate, DateTimeKind.Utc),
            Input.EstimatedArrival.HasValue ? DateTime.SpecifyKind(Input.EstimatedArrival.Value, DateTimeKind.Utc) : null,
            Input.Status,
            Input.LoadingPort,
            Input.DischargePort,
            Input.Notes,
            Input.CustomerId);
        shipment.ReassignCustomer(Input.CustomerId);

        var stageStartedAt = DateTime.SpecifyKind(Input.StageStartedAt, DateTimeKind.Utc);
        DateTime? stageCompletedAt = Input.StageCompletedAt.HasValue
            ? DateTime.SpecifyKind(Input.StageCompletedAt.Value, DateTimeKind.Utc)
            : null;

        shipment.SetOrUpdateStage(Input.Status, stageStartedAt, stageCompletedAt, Input.StageNotes);

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

    private static DateTime ToLocal(DateTime value)
    {
        var utcValue = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

        return utcValue.ToLocalTime();
    }

    public sealed class ShipmentInput
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid SupplierId { get; set; }

        public Guid? CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime ShipmentDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EstimatedArrival { get; set; }

        [Required]
        public ShipmentStatus Status { get; set; }

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


