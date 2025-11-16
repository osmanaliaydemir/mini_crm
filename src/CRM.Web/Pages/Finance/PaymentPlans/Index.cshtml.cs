using CRM.Application.Common.Pagination;
using CRM.Application.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Finance.PaymentPlans;

public class IndexModel : PageModel
{
    private readonly IPaymentPlanService _paymentPlanService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IPaymentPlanService paymentPlanService, ILogger<IndexModel> logger)
    {
        _paymentPlanService = paymentPlanService;
        _logger = logger;
    }

    public string? CustomerFilter { get; set; }
    public PagedResult<PaymentPlanListItemDto>? PagedPlans { get; private set; }
    public IReadOnlyList<PaymentPlanListItemDto> Plans => PagedPlans?.Items ?? Array.Empty<PaymentPlanListItemDto>();

    public async Task OnGetAsync(string? customer, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        CustomerFilter = customer;

        try
        {
            var pagination = PaginationRequest.Create(page, pageSize);
            PagedPlans = await _paymentPlanService.GetAllPagedAsync(pagination, customer, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment plans");
            PagedPlans = new PagedResult<PaymentPlanListItemDto>(Array.Empty<PaymentPlanListItemDto>(), 0, 1, pageSize);
        }
    }

    // DataTables server-side processing için handler
    public async Task<IActionResult> OnGetDataAsync(
        [FromQuery] int draw,
        [FromQuery] int start,
        [FromQuery] int length,
        [FromQuery] string? customer,
        [FromQuery(Name = "search[value]")] string? searchValue,
        [FromQuery(Name = "order[0][column]")] int? orderColumn,
        [FromQuery(Name = "order[0][dir]")] string? orderDir,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var page = (start / length) + 1;
            var pagination = PaginationRequest.Create(page, length);

            // Column mapping: DataTables column index -> sort column name
            var columnMapping = new Dictionary<int, string>
            {
                { 0, "id" },
                { 1, "customername" },
                { 2, "shipmentreference" },
                { 3, "plantype" },
                { 4, "totalamount" },
                { 5, "startdate" }
            };

            // Determine sort column and direction
            string? sortColumn = null;
            string? sortDirection = null;
            
            if (orderColumn.HasValue && columnMapping.TryGetValue(orderColumn.Value, out var mappedColumn))
            {
                sortColumn = mappedColumn;
                sortDirection = orderDir?.ToLower() == "asc" ? "asc" : "desc";
            }

            // Use search value if provided, otherwise fall back to customer filter
            var search = !string.IsNullOrWhiteSpace(searchValue) ? searchValue : customer;

            var pagedResult = await _paymentPlanService.GetAllPagedAsync(pagination, search, sortColumn, sortDirection, cancellationToken);

            // DataTables formatında response
            return new JsonResult(new
            {
                draw = draw,
                recordsTotal = pagedResult.TotalCount,
                recordsFiltered = pagedResult.TotalCount,
                data = pagedResult.Items.Select(plan => new object[]
                {
                    plan.Id.ToString(),
                    plan.CustomerName,
                    plan.ShipmentReference,
                    plan.PlanType.ToString(),
                    plan.TotalAmount.ToString("C2") + " " + plan.Currency,
                    plan.StartDate.ToString("d"),
                    $"<a class=\"btn btn-icon\" href=\"/Finance/PaymentPlans/Details/{plan.Id}\" title=\"Details\"><img src=\"/images/detail.png\" alt=\"Details\" width=\"18\" height=\"18\" loading=\"lazy\" /></a>"
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment plans data for DataTables");
            return new JsonResult(new
            {
                draw = draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<object[]>()
            });
        }
    }
}

