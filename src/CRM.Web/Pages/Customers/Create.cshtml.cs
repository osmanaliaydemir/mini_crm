using System.ComponentModel.DataAnnotations;
using CRM.Domain.Customers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Customers;

public class CreateModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public CreateModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public CustomerInput Customer { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var entity = new Customer(
            Guid.NewGuid(),
            Customer.Name,
            Customer.LegalName,
            Customer.TaxNumber,
            Customer.Email,
            Customer.Phone,
            Customer.Address,
            Customer.Segment,
            Customer.Notes);

        entity.Update(
            Customer.Name,
            Customer.LegalName,
            Customer.TaxNumber,
            Customer.Email,
            Customer.Phone,
            Customer.Address,
            Customer.Segment,
            Customer.Notes);

        entity.ClearContacts();
        if (!string.IsNullOrWhiteSpace(Customer.PrimaryContactName))
        {
            entity.AddContact(
                Customer.PrimaryContactName,
                Customer.PrimaryContactEmail,
                Customer.PrimaryContactPhone,
                Customer.PrimaryContactPosition);
        }

        _dbContext.Customers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }

    public sealed class CustomerInput
    {
        [Display(Name = "Müşteri Adı")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Ticari Unvan")]
        [MaxLength(200)]
        public string? LegalName { get; set; }

        [Display(Name = "Vergi No")]
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress]
        [MaxLength(200)]
        public string? Email { get; set; }

        [Display(Name = "Telefon")]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [Display(Name = "Adres")]
        [MaxLength(300)]
        public string? Address { get; set; }

        [Display(Name = "Segment")]
        [MaxLength(100)]
        public string? Segment { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "İrtibat Adı")]
        [MaxLength(150)]
        public string? PrimaryContactName { get; set; }

        [Display(Name = "İrtibat Ünvanı")]
        [MaxLength(100)]
        public string? PrimaryContactPosition { get; set; }

        [Display(Name = "İrtibat E-posta")]
        [EmailAddress]
        [MaxLength(200)]
        public string? PrimaryContactEmail { get; set; }

        [Display(Name = "İrtibat Telefon")]
        [MaxLength(50)]
        public string? PrimaryContactPhone { get; set; }
    }
}

