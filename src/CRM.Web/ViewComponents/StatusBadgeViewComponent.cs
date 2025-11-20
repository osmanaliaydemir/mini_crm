using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.ViewComponents;

public class StatusBadgeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string label, string statusType, string? cssClass = null)
    {
        var model = new StatusBadgeViewModel
        {
            Label = label,
            StatusType = statusType, // success, warning, info, danger, neutral, positive, negative
            CssClass = cssClass
        };

        return View(model);
    }

    public class StatusBadgeViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string StatusType { get; set; } = "neutral";
        public string? CssClass { get; set; }
    }
}

