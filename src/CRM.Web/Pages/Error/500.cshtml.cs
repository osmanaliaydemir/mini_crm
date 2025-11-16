using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Error;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class InternalServerErrorModel : PageModel
{
    public int ErrorStatusCode { get; set; } = 500;
    public string? RequestId { get; set; }
    public bool ShowRequestId { get; set; }

    public void OnGet()
    {
        ErrorStatusCode = 500;
        RequestId = HttpContext.TraceIdentifier;
        ShowRequestId = !string.IsNullOrEmpty(RequestId);
    }
}

