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
        [RequestFormLimits(MultipartBodyLengthLimit = 8000001, MultipartHeadersLengthLimit = 42)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
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
        [RequestFormLimits(MultipartBodyLengthLimit = 42)] // Compliant value is below limit.
        public ActionResult MultipartFormRequestBelowLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = "NonNumericalValue")] // Error [CS0029]
        public ActionResult MultipartFormRequestNonNumerical()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits]
        public ActionResult MultipartFormRequestWithoutParams()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(42)] // Error [CS1729]
        public ActionResult MultipartFormRequestNonNameEquals()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit("NonNumerical")] // Error [CS1503]
        public ActionResult PostRequestNonNumerical()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit] // Error [CS7036]
        public ActionResult PostRequestWithoutParams()
        {
            return null;
        }

        public ActionResult MethodWithoutAttributes()
        {
            return null;
        }
    }
}
