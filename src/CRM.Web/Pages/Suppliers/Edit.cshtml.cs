using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Suppliers;

public class EditModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public EditModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public SupplierInput Supplier { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        Supplier = new SupplierInput
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Country = supplier.Country,
            TaxNumber = supplier.TaxNumber,
            ContactEmail = supplier.ContactEmail,
            ContactPhone = supplier.ContactPhone,
            AddressLine = supplier.AddressLine,
            Notes = supplier.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == Supplier.Id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        supplier.Update(
            Supplier.Name,
            Supplier.Country,
            Supplier.TaxNumber,
            Supplier.ContactEmail,
            Supplier.ContactPhone,
            Supplier.AddressLine,
            Supplier.Notes);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }

    public sealed class SupplierInput
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [Display(Name = "Ad")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Ülke")]
        [MaxLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Vergi No")]
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress]
        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        [Display(Name = "Telefon")]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [Display(Name = "Adres")]
        [MaxLength(300)]
        public string? AddressLine { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

