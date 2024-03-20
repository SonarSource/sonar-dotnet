using Microsoft.AspNetCore.Mvc;

namespace NetCore31.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
