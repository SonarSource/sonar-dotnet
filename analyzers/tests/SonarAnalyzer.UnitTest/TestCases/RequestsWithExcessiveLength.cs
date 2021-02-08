using System;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Diagnostics
{
    public class MyController : Controller
    {
        [HttpPost]
        [DisableRequestSizeLimit] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult PostRequestWithNoLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(10000000)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult PostRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(1623)] // Compliant value is below limit.
        public ActionResult PostRequestBelowLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 8000001)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult MultipartFormRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 42)] // Compliant value is below limit.
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
