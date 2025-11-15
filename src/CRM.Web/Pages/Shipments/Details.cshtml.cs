using CRM.Application.Shipments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Shipments;

public class DetailsModel : PageModel
{
    private readonly IShipmentService _shipmentService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IShipmentService shipmentService, ILogger<DetailsModel> logger)
    {
        _shipmentService = shipmentService;
        _logger = logger;
    }

    public ShipmentDetailsDto? Shipment { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Shipment = await _shipmentService.GetDetailsByIdAsync(id, cancellationToken);
        if (Shipment is null)
        {
            return NotFound();
        }

        return Page();
    }
}


