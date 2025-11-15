using System.ComponentModel.DataAnnotations;
using CRM.Application.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Customers;

public class EditModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(ICustomerService customerService, ILogger<EditModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [BindProperty]
    public CustomerInput Customer { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);

        if (customer is null)
        {
            return NotFound();
        }

        // Map DTO to Input
        Customer = new CustomerInput
        {
            Id = customer.Id,
            Name = customer.Name,
            LegalName = customer.LegalName,
            TaxNumber = customer.TaxNumber,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            Segment = customer.Segment,
            Notes = customer.Notes,
            PrimaryContactName = customer.PrimaryContactName,
            PrimaryContactEmail = customer.PrimaryContactEmail,
            PrimaryContactPhone = customer.PrimaryContactPhone,
            PrimaryContactPosition = customer.PrimaryContactPosition
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new UpdateCustomerRequest(
                Customer.Id,
                Customer.Name,
                Customer.LegalName,
                Customer.TaxNumber,
                Customer.Email,
                Customer.Phone,
                Customer.Address,
                Customer.Segment,
                Customer.Notes,
                Customer.PrimaryContactName,
                Customer.PrimaryContactEmail,
                Customer.PrimaryContactPhone,
                Customer.PrimaryContactPosition);

            await _customerService.UpdateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Müşteri başarıyla güncellendi.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Customer not found: {CustomerId}", Customer.Id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", Customer.Id);
            ModelState.AddModelError(string.Empty, "Müşteri güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
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

