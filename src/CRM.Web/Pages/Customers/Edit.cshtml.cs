using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Customers;

public class EditModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public EditModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public CustomerInput Customer { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var primaryContact = entity.Contacts.FirstOrDefault();

        Customer = new CustomerInput
        {
            Id = entity.Id,
            Name = entity.Name,
            LegalName = entity.LegalName,
            TaxNumber = entity.TaxNumber,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            Segment = entity.Segment,
            Notes = entity.Notes,
            PrimaryContactName = primaryContact?.FullName,
            PrimaryContactEmail = primaryContact?.Email,
            PrimaryContactPhone = primaryContact?.Phone,
            PrimaryContactPosition = primaryContact?.Position
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var entity = await _dbContext.Customers
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == Customer.Id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }

    public sealed class CustomerInput
    {
        [HiddenInput]
        public Guid Id { get; set; }

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

