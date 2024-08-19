using System.Web.Mvc;

namespace AspNet4x.Controllers
{
    [Route(@"A\[controller]")]    // Noncompliant (S6930)
    //     ^^^^^^^^^^^^^^^^^
    public class S6930Controller : Controller
    {
        [Route(@"A\[action]")]    // Noncompliant (S6930)
        //     ^^^^^^^^^^^^^
        public ActionResult Index() => View();
    }
}
