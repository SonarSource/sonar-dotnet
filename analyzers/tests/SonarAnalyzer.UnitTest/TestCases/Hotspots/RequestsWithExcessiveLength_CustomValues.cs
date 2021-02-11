using System;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Diagnostics
{
    public class MyController : Controller
    {
        [HttpPost]
        [RequestSizeLimit(16)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^
        public ActionResult PostRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 23)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult MultipartFormRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(14)]
        public ActionResult PostRequestBelowLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 21)]
        public ActionResult MultipartFormRequestBelowLimit()
        {
            return null;
        }
    }
}
