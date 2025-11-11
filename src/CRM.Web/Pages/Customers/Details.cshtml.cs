using System.ComponentModel.DataAnnotations;
using CRM.Domain.Customers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Customers;

public class DetailsModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public DetailsModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Customer? Customer { get; private set; }
    public IList<CustomerContact> Contacts { get; private set; } = new List<CustomerContact>();
    public IList<CustomerInteraction> Interactions { get; private set; } = new List<CustomerInteraction>();

    [BindProperty]
    public InteractionInput InteractionForm { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        await LoadDataAsync(id, cancellationToken);
        if (Customer is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadDataAsync(InteractionForm.CustomerId, cancellationToken);
        if (Customer is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var interaction = new CustomerInteraction(
            InteractionForm.CustomerId,
            InteractionForm.InteractionDate,
            InteractionForm.InteractionType,
            InteractionForm.Subject,
            InteractionForm.Notes,
            InteractionForm.RecordedBy);

        _dbContext.CustomerInteractions.Add(interaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage(new { id = InteractionForm.CustomerId });
    }

    private async Task LoadDataAsync(Guid customerId, CancellationToken cancellationToken)
    {
        Customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        Contacts = await _dbContext.CustomerContacts.AsNoTracking()
            .Where(contact => contact.CustomerId == customerId)
            .OrderBy(contact => contact.FullName)
            .ToListAsync(cancellationToken);

        Interactions = await _dbContext.CustomerInteractions.AsNoTracking()
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.InteractionDate)
            .ToListAsync(cancellationToken);

        InteractionForm.CustomerId = customerId;
        if (InteractionForm.InteractionDate == default)
        {
            InteractionForm.InteractionDate = DateTime.UtcNow;
        }
    }

    public sealed class InteractionInput
    {
        [HiddenInput]
        public Guid CustomerId { get; set; }

        [Display(Name = "İşlem Tarihi")]
        [Required]
        public DateTime InteractionDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Etkileşim Tipi")]
        [Required]
        [MaxLength(100)]
        public string InteractionType { get; set; } = string.Empty;

        [Display(Name = "Konu")]
        [MaxLength(200)]
        public string? Subject { get; set; }

        [Display(Name = "Notlar")]
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Kaydeden")]
        [MaxLength(100)]
        public string? RecordedBy { get; set; }
    }
}

