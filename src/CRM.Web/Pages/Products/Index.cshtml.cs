using CRM.Application.Common.Pagination;
using CRM.Application.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IProductService productService, ILogger<IndexModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public string? Search { get; set; }

    public async Task OnGetAsync(string? search, CancellationToken cancellationToken)
    {
        Search = search;
    }

    // DataTables server-side processing için handler
    public async Task<IActionResult> OnGetDataAsync(
        [FromQuery] int draw,
        [FromQuery] int start,
        [FromQuery] int length,
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
                { 0, "name" },
                { 1, "species" },
                { 2, "grade" },
                { 3, "unitofmeasure" }
            };

            // Determine sort column and direction
            string? sortColumn = null;
            string? sortDirection = null;
            
            if (orderColumn.HasValue && columnMapping.TryGetValue(orderColumn.Value, out var mappedColumn))
            {
                sortColumn = mappedColumn;
                sortDirection = orderDir?.ToLower() == "asc" ? "asc" : "desc";
            }

            var search = !string.IsNullOrWhiteSpace(searchValue) ? searchValue : null;
            var pagedResult = await _productService.GetAllPagedAsync(pagination, search, cancellationToken);

            // DataTables formatında response
            return new JsonResult(new
            {
                draw = draw,
                recordsTotal = pagedResult.TotalCount,
                recordsFiltered = pagedResult.TotalCount,
                data = pagedResult.Items.Select(product => new object[]
                {
                    product.Name,
                    product.Species ?? "-",
                    product.Grade ?? "-",
                    product.UnitOfMeasure,
                    $"<a class=\"btn btn-icon\" href=\"/Products/Details/{product.Id}\" title=\"Detay\"><img src=\"/images/detail.png\" alt=\"Detay\" width=\"18\" height=\"18\" loading=\"lazy\" /></a>" +
                    $"<a class=\"btn btn-icon\" href=\"/Products/Edit/{product.Id}\" title=\"Düzenle\"><img src=\"/images/edit.png\" alt=\"Düzenle\" width=\"18\" height=\"18\" loading=\"lazy\" /></a>" +
                    $"<a class=\"btn btn-icon\" href=\"/Products/Delete/{product.Id}\" title=\"Sil\"><img src=\"/images/delete.png\" alt=\"Sil\" width=\"18\" height=\"18\" loading=\"lazy\" /></a>"
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products data for DataTables");
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

