using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Application.Finance;
using CRM.Domain.Finance;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRM.Web.Pages.Finance.Cashbox;

public class IndexModel : PageModel
{
    private readonly ICashTransactionService _cashTransactionService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ICashTransactionService cashTransactionService, ILogger<IndexModel> logger)
    {
        _cashTransactionService = cashTransactionService;
        _logger = logger;
        TransactionTypeOptions = Enum.GetValues(typeof(CashTransactionType))
            .Cast<CashTransactionType>()
            .Select(type => new SelectListItem(type.ToString(), type.ToString()))
            .ToList();
    }

    public FilterModel Filter { get; private set; } = new();
    public IReadOnlyList<CashTransactionDto> Transactions { get; private set; } = Array.Empty<CashTransactionDto>();
    public CashSummary Summary { get; private set; } = new(0, 0);
    public IList<SelectListItem> TransactionTypeOptions { get; }
    public string MonthlyLabelsJson { get; private set; } = "[]";
    public string MonthlyIncomeDataJson { get; private set; } = "[]";
    public string MonthlyExpenseDataJson { get; private set; } = "[]";
    public string MonthlyNetDataJson { get; private set; } = "[]";
    public IReadOnlyList<CategorySummary> CategorySummaries { get; private set; } = Array.Empty<CategorySummary>();

    public async Task OnGetAsync(DateTime? from, DateTime? to, string? type, CancellationToken cancellationToken)
    {
        Filter = new FilterModel
        {
            From = from,
            To = to,
            Type = type
        };

        try
        {
            CashTransactionType? parsedType = null;
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<CashTransactionType>(type, out var parsed))
            {
                parsedType = parsed;
            }

            var dashboardData = await _cashTransactionService.GetDashboardDataAsync(from, to, parsedType, cancellationToken);

            Transactions = dashboardData.Transactions;
            Summary = dashboardData.Summary;
            CategorySummaries = dashboardData.CategorySummaries;

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            MonthlyLabelsJson = JsonSerializer.Serialize(dashboardData.MonthlyLabels, jsonOptions);
            MonthlyIncomeDataJson = JsonSerializer.Serialize(dashboardData.MonthlyIncomeData, jsonOptions);
            MonthlyExpenseDataJson = JsonSerializer.Serialize(dashboardData.MonthlyExpenseData, jsonOptions);
            MonthlyNetDataJson = JsonSerializer.Serialize(dashboardData.MonthlyNetData, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cash transaction dashboard data");
            Transactions = Array.Empty<CashTransactionDto>();
            Summary = new CashSummary(0, 0);
        }
    }

    public sealed class FilterModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Type { get; set; }
    }
}

