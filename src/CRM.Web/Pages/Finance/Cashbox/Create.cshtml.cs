using System.ComponentModel.DataAnnotations;
using CRM.Domain.Finance;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Finance.Cashbox;

public class CreateModel : PageModel
{
    private readonly CRMDbContext _dbContext;

    public CreateModel(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
        TransactionTypeOptions = Enum.GetValues(typeof(CashTransactionType))
            .Cast<CashTransactionType>()
            .Select(type => new SelectListItem(type.ToString(), type.ToString()))
            .ToList();
    }

    [BindProperty]
    public CashInput Input { get; set; } = new();

    public IList<SelectListItem> CustomerOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> ShipmentOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> TransactionTypeOptions { get; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var transaction = new CashTransaction(
            Guid.NewGuid(),
            Input.TransactionDate,
            Input.TransactionType,
            Input.Amount,
            Input.Currency ?? "TRY",
            Input.Description,
            Input.Category,
            Input.RelatedCustomerId,
            Input.RelatedShipmentId);

        _dbContext.CashTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("Index");
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers
            .Select(customer => new { customer.Id, customer.Name })
            .ToListAsync(cancellationToken);

        CustomerOptions = customers
            .Select(customer => new SelectListItem(customer.Name, customer.Id.ToString()))
            .ToList();

        var shipments = await _dbContext.Shipments
            .Select(shipment => new { shipment.Id, shipment.ReferenceNumber })
            .ToListAsync(cancellationToken);

        ShipmentOptions = shipments
            .Select(shipment => new SelectListItem(shipment.ReferenceNumber, shipment.Id.ToString()))
            .ToList();

        if (Input.TransactionDate == default)
        {
            Input.TransactionDate = DateTime.UtcNow;
        }
    }

    public sealed class CashInput
    {
        [Display(Name = "Tarih")]
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Tip")]
        [Required]
        public CashTransactionType TransactionType { get; set; } = CashTransactionType.Income;

        [Display(Name = "Tutar")]
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Display(Name = "Para Birimi")]
        [MaxLength(10)]
        public string? Currency { get; set; } = "TRY";

        [Display(Name = "Kategori")]
        [MaxLength(150)]
        public string? Category { get; set; }

        [Display(Name = "Açıklama")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "İlgili Müşteri")]
        public Guid? RelatedCustomerId { get; set; }

        [Display(Name = "İlgili Sevkiyat")]
        public Guid? RelatedShipmentId { get; set; }
    }
}

