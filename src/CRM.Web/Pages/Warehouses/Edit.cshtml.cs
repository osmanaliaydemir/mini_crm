using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Warehouses;

public class EditModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public EditModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public WarehouseInput Warehouse { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
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
            Notes = warehouse.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(w => w.Id == Warehouse.Id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        warehouse.Update(Warehouse.Name, Warehouse.Location, Warehouse.ContactPerson, Warehouse.ContactPhone, Warehouse.Notes);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
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
    }
}

