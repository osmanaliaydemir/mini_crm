using System.ComponentModel.DataAnnotations;
using CRM.Domain.Suppliers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Suppliers;

public class CreateModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public CreateModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public SupplierInput Supplier { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var entity = new Supplier(
            Guid.NewGuid(),
            Supplier.Name,
            Supplier.Country,
            Supplier.TaxNumber,
            Supplier.ContactEmail,
            Supplier.ContactPhone,
            Supplier.AddressLine);

        entity.Update(
            Supplier.Name,
            Supplier.Country,
            Supplier.TaxNumber,
            Supplier.ContactEmail,
            Supplier.ContactPhone,
            Supplier.AddressLine,
            Supplier.Notes);

        _dbContext.Suppliers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }

    public sealed class SupplierInput
    {
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

