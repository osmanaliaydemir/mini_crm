using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.ViewComponents;

public class ActionButtonsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string? detailsPage = null, string? editPage = null, string? deletePage = null,
        Guid? detailsId = null, Guid? editId = null, Guid? deleteId = null, string? detailsTitle = null,
        string? editTitle = null, string? deleteTitle = null)
    {
        var model = new ActionButtonsViewModel
        {
            DetailsPage = detailsPage,
            EditPage = editPage,
            DeletePage = deletePage,
            DetailsId = detailsId,
            EditId = editId,
            DeleteId = deleteId,
            DetailsTitle = detailsTitle ?? "Detay",
            EditTitle = editTitle ?? "Düzenle",
            DeleteTitle = deleteTitle ?? "Sil"
        };

        return View(model);
    }

    public class ActionButtonsViewModel
    {
        public string? DetailsPage { get; set; }
        public string? EditPage { get; set; }
        public string? DeletePage { get; set; }
        public Guid? DetailsId { get; set; }
        public Guid? EditId { get; set; }
        public Guid? DeleteId { get; set; }
        public string DetailsTitle { get; set; } = "Detay";
        public string EditTitle { get; set; } = "Düzenle";
        public string DeleteTitle { get; set; } = "Sil";
    }
}

