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
        [RequestSizeLimit(43)]
        public ActionResult PostRequestAboveLimit() =>
            null;

        [HttpPost]
        [RequestSizeLimit(41)]
        public ActionResult PostRequestBelowLimit() =>
            null;

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 43)]
        public ActionResult MultipartFormRequestAboveLimit() =>
            null;

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 41)]
        public ActionResult MultipartFormRequestBelowLimit() =>
            null;
    }
}
