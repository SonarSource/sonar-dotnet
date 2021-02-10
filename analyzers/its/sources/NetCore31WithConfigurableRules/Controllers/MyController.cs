using Microsoft.AspNetCore.Mvc;

namespace NetCore31WithConfigurableRules.Controllers
{
    public class MyController : Controller
    {
        [HttpPost]
        [DisableRequestSizeLimit]
        public ActionResult PostRequestWithNoLimit() =>
            null;

        [HttpPost]
        [RequestSizeLimit(17)]
        public ActionResult PostRequestAboveLimit() =>
            null;

        [HttpPost]
        [RequestSizeLimit(15)]
        public ActionResult PostRequestBelowLimit() =>
            null;

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 24)]
        public ActionResult MultipartFormRequestAboveLimit() =>
            null;

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 22)]
        public ActionResult MultipartFormRequestBelowLimit() =>
            null;
    }
}
