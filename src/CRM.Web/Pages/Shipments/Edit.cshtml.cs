using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Application.Shipments;
using CRM.Application.Suppliers;
using CRM.Application.Customers;
using CRM.Application.Common;
using CRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Shipments;

public class EditModel : PageModel
{
    private readonly IShipmentService _shipmentService;
    private readonly ISupplierService _supplierService;
    private readonly ICustomerService _customerService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EditModel> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public EditModel(IShipmentService shipmentService, ISupplierService supplierService, ICustomerService customerService,
        IApplicationDbContext context, ILogger<EditModel> logger, IStringLocalizer<SharedResource> localizer)
    {
        _shipmentService = shipmentService;
        _supplierService = supplierService;
        _customerService = customerService;
        _context = context;
        _logger = logger;
        _localizer = localizer;
    }

    [BindProperty]
    public ShipmentInput Input { get; set; } = new();

    public IList<SelectListItem> SupplierOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> CustomerOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> StatusOptions { get; private set; } = new List<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);

        var shipment = await _shipmentService.GetByIdAsync(id, cancellationToken);
        if (shipment is null)
        {
            return NotFound();
        }

        // Stage bilgilerini almak için context kullanıyoruz
        var shipmentWithStages = await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        var latestStage = shipmentWithStages?.Stages
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
            StageNotes = latestStage?.Notes,
            RowVersion = shipment.RowVersion
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

        try
        {
            var request = new UpdateShipmentRequest(Input.Id, Input.SupplierId, Input.CustomerId, Input.ReferenceNumber,
                Input.ShipmentDate, Input.EstimatedArrival, Input.Status, Input.LoadingPort,
                Input.DischargePort, Input.Notes, Input.StageStartedAt,
                Input.StageCompletedAt, Input.StageNotes, Input.RowVersion);

            await _shipmentService.UpdateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Sevkiyat başarıyla güncellendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Details", new { id = Input.Id });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when updating shipment: {ShipmentId}", Input.Id);
            ModelState.AddModelError(string.Empty, _localizer["Error_ConcurrencyConflict"]);
            // Reload the shipment to get the latest data
            var shipment = await _shipmentService.GetByIdAsync(Input.Id, cancellationToken);
            if (shipment != null)
            {
                Input.RowVersion = shipment.RowVersion;
            }
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Shipment not found: {ShipmentId}", Input.Id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipment: {ShipmentId}", Input.Id);
            ModelState.AddModelError(string.Empty, "Sevkiyat güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var suppliers = await _supplierService.GetAllAsync(cancellationToken: cancellationToken);
        SupplierOptions = suppliers.Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();

        var customers = await _customerService.GetAllAsync(cancellationToken: cancellationToken);
        CustomerOptions = customers.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

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

        [HiddenInput]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}


