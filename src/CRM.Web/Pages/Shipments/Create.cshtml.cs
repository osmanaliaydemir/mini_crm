using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Application.Shipments;
using CRM.Application.Suppliers;
using CRM.Application.Customers;
using CRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRM.Web.Pages.Shipments;

public class CreateModel : PageModel
{
    private readonly IShipmentService _shipmentService;
    private readonly ISupplierService _supplierService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IShipmentService shipmentService, ISupplierService supplierService, ICustomerService customerService, ILogger<CreateModel> logger)
    {
        _shipmentService = shipmentService;
        _supplierService = supplierService;
        _customerService = customerService;
        _logger = logger;
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

        try
        {
            var request = new CreateShipmentRequest(
                Input.SupplierId,
                Input.CustomerId,
                Input.ReferenceNumber.Trim(),
                Input.ShipmentDate,
                Input.EstimatedArrival,
                Input.Status,
                Input.LoadingPort,
                Input.DischargePort,
                Input.Notes,
                Input.StageStartedAt,
                Input.StageCompletedAt,
                Input.StageNotes);

            var shipmentId = await _shipmentService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Sevkiyat başarıyla oluşturuldu.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Details", new { id = shipmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment");
            ModelState.AddModelError(string.Empty, "Sevkiyat oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
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


