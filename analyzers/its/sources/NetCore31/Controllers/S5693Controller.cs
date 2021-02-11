using Microsoft.AspNetCore.Mvc;

namespace NetCore31.Controllers
{
    public class S5693Controller
    {
        [HttpPost]
        [DisableRequestSizeLimit]
        public ActionResult PostRequestWithNoLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public ActionResult PostRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(1623)]
        public ActionResult PostRequestBelowLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 8_000_001, MultipartHeadersLengthLimit = 42)]
        public ActionResult MultipartFormRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartHeadersLengthLimit = 55)]
        public ActionResult MultipartFormRequestHeadersLimitSet()
        {
            return null;
        }

        [HttpPost]
        public ActionResult MultiPartFromRequestWithDefaultLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 42)]
        public ActionResult MultipartFormRequestBelowLimit()
        {
            return null;
        }

        public ActionResult MethodWithoutAttributes()
        {
            return null;
        }
    }
}
