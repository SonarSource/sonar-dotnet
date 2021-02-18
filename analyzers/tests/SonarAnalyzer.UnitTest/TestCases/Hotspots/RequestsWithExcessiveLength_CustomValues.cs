using System;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Diagnostics
{
    public class MyController : Controller
    {
        [HttpPost]
        [RequestSizeLimit(43)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^
        public ActionResult PostRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 43)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult MultipartFormRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(41)]
        public ActionResult PostRequestBelowLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 41)]
        public ActionResult MultipartFormRequestBelowLimit()
        {
            return null;
        }
    }
}
