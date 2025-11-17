using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Application.Finance;
using CRM.Application.Customers;
using CRM.Application.Shipments;
using CRM.Domain.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Finance.Cashbox;

public class CreateModel : PageModel
{
    private readonly ICashTransactionService _cashTransactionService;
    private readonly ICustomerService _customerService;
    private readonly IShipmentService _shipmentService;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ICashTransactionService cashTransactionService, ICustomerService customerService,
        IShipmentService shipmentService, IStringLocalizer<SharedResource> localizer, ILogger<CreateModel> logger)
    {
        _cashTransactionService = cashTransactionService;
        _customerService = customerService;
        _shipmentService = shipmentService;
        _localizer = localizer;
        _logger = logger;
    }

    [BindProperty]
    public CashInput Input { get; set; } = new();

    public IList<SelectListItem> CustomerOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> ShipmentOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> TransactionTypeOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> CurrencyOptions { get; private set; } = new List<SelectListItem>();
    public IList<SelectListItem> CategoryOptions { get; private set; } = new List<SelectListItem>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        LoadStaticOptions();
        await LoadLookupsAsync(cancellationToken);
    }

    private void LoadStaticOptions()
    {
        // Transaction Type Options with localization
        TransactionTypeOptions = Enum.GetValues(typeof(CashTransactionType))
            .Cast<CashTransactionType>()
            .Select(type => new SelectListItem(
                _localizer[$"Finance_Cashbox_Type_{type}"],
                type.ToString()))
            .ToList();

        // Currency Options
        CurrencyOptions = new List<SelectListItem>
        {
            new(_localizer["Finance_Cashbox_Form_Currency_TL"], "TRY"),
            new(_localizer["Finance_Cashbox_Form_Currency_USD"], "USD"),
            new(_localizer["Finance_Cashbox_Form_Currency_EUR"], "EUR")
        };

        // Category Options
        CategoryOptions = new List<SelectListItem>
        {
            new(_localizer["Finance_Cashbox_Form_Category_Select"], ""),
            new(_localizer["Finance_Cashbox_Form_Category_Sales"], _localizer["Finance_Cashbox_Form_Category_Sales"].Value),
            new(_localizer["Finance_Cashbox_Form_Category_Purchase"], _localizer["Finance_Cashbox_Form_Category_Purchase"].Value),
            new(_localizer["Finance_Cashbox_Form_Category_Transport"], _localizer["Finance_Cashbox_Form_Category_Transport"].Value),
            new(_localizer["Finance_Cashbox_Form_Category_Warehouse"], _localizer["Finance_Cashbox_Form_Category_Warehouse"].Value),
            new(_localizer["Finance_Cashbox_Form_Category_Customs"], _localizer["Finance_Cashbox_Form_Category_Customs"].Value),
            new(_localizer["Finance_Cashbox_Form_Category_Tax"], _localizer["Finance_Cashbox_Form_Category_Tax"].Value),
            new(_localizer["Finance_Cashbox_Form_Category_Insurance"], _localizer["Finance_Cashbox_Form_Category_Insurance"].Value),
            new(_localizer["Finance_Cashbox_Form_Category_Other"], _localizer["Finance_Cashbox_Form_Category_Other"].Value)
        };
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        LoadStaticOptions();
        await LoadLookupsAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new CreateCashTransactionRequest(
                Input.TransactionDate,
                Input.TransactionType,
                Input.Amount,
                Input.Currency,
                Input.Description,
                Input.Category,
                Input.RelatedCustomerId,
                Input.RelatedShipmentId);

            await _cashTransactionService.CreateAsync(request, cancellationToken);

            TempData["StatusMessage"] = "Nakit işlemi başarıyla oluşturuldu.";
            TempData["StatusMessageType"] = "success";

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cash transaction");
            ModelState.AddModelError(string.Empty, "Nakit işlemi oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
            return Page();
        }
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken: cancellationToken);
        CustomerOptions = customers
            .Select(customer => new SelectListItem(customer.Name, customer.Id.ToString()))
            .ToList();

        var shipments = await _shipmentService.GetAllAsync(cancellationToken: cancellationToken);
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
        [Display(Name = "Finance_Cashbox_Form_Field_Date")]
        [Required(ErrorMessage = "RequiredAttribute_ValidationError")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Finance_Cashbox_Form_Field_Type")]
        [Required(ErrorMessage = "RequiredAttribute_ValidationError")]
        public CashTransactionType TransactionType { get; set; } = CashTransactionType.Income;

        [Display(Name = "Finance_Cashbox_Form_Field_Amount")]
        [Required(ErrorMessage = "RequiredAttribute_ValidationError")]
        [Range(0.01, double.MaxValue, ErrorMessage = "RangeAttribute_ValidationError")]
        public decimal Amount { get; set; }

        [Display(Name = "Finance_Cashbox_Form_Field_Currency")]
        [Required(ErrorMessage = "RequiredAttribute_ValidationError")]
        [MaxLength(10, ErrorMessage = "MaxLengthAttribute_ValidationError")]
        public string Currency { get; set; } = "TRY";

        [Display(Name = "Finance_Cashbox_Form_Field_Category")]
        [MaxLength(150, ErrorMessage = "MaxLengthAttribute_ValidationError")]
        public string? Category { get; set; }

        [Display(Name = "Finance_Cashbox_Form_Field_Description")]
        [MaxLength(500, ErrorMessage = "MaxLengthAttribute_ValidationError")]
        public string? Description { get; set; }

        [Display(Name = "Finance_Cashbox_Form_Field_RelatedCustomer")]
        public Guid? RelatedCustomerId { get; set; }

        [Display(Name = "Finance_Cashbox_Form_Field_RelatedShipment")]
        public Guid? RelatedShipmentId { get; set; }
    }
}

