using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Error;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class NotFoundModel : PageModel
{
    public int ErrorStatusCode { get; set; } = 404;
    public string? OriginalPath { get; set; }

    public void OnGet(string? code = null)
    {
        ErrorStatusCode = 404;
        OriginalPath = Request.Query["originalPath"].ToString();
    }
}

