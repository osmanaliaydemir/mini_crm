using System.ComponentModel.DataAnnotations;
using CRM.Application.Warehouses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Warehouses;

public class CreateModel : PageModel
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IWarehouseService warehouseService, ILogger<CreateModel> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    [BindProperty]
    public WarehouseInput Warehouse { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new CreateWarehouseRequest(Warehouse.Name, Warehouse.Location,
                Warehouse.ContactPerson, Warehouse.ContactPhone, Warehouse.Notes);

            await _warehouseService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Depo başarıyla oluşturuldu.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warehouse");
            ModelState.AddModelError(string.Empty, "Depo oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    public sealed class WarehouseInput
    {
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
    }
}

