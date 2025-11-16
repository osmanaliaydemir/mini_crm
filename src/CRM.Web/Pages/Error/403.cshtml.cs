using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Error;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class ForbiddenModel : PageModel
{
    public int ErrorStatusCode { get; set; } = 403;
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
        ErrorStatusCode = 403;
        ReturnUrl = Request.Query["returnUrl"].ToString();
    }
}

