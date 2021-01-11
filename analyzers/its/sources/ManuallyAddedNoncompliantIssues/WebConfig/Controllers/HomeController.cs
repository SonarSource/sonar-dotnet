// Noncompliant (S1451)

using System.Web.Mvc;

namespace Framework48.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            var model = new Models.Article("TitleFromController", "TextFromController");
            return View(model);
        }
    }
}
