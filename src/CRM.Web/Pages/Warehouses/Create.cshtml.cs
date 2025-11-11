using System.ComponentModel.DataAnnotations;
using CRM.Domain.Warehouses;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Warehouses;

public class CreateModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public CreateModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
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

        var warehouse = new Warehouse(
            Guid.NewGuid(),
            Warehouse.Name,
            Warehouse.Location,
            Warehouse.ContactPerson,
            Warehouse.ContactPhone);

        warehouse.Update(
            Warehouse.Name,
            Warehouse.Location,
            Warehouse.ContactPerson,
            Warehouse.ContactPhone,
            Warehouse.Notes);

        _dbContext.Warehouses.Add(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
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

