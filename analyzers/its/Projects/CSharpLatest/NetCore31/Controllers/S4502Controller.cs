using Microsoft.AspNetCore.Mvc;

namespace NetCore31.Controllers
{
    [IgnoreAntiforgeryToken]
    public class S4502Controller
    {
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public ActionResult Action() => null;
    }
}
