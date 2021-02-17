using System;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Diagnostics
{
    public class MyController : Controller
    {
        [HttpPost]
        [DisableRequestSizeLimit] // Noncompliant {{Make sure the content length limit is safe here.}}
        public ActionResult PostRequestWithNoLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(8_000_001)] // Noncompliant
        public ActionResult RequestSizeLimitAboveLimit()
        {
            [RequestFormLimits(MultipartBodyLengthLimit = 8_000_001)] // Noncompliant
            ActionResult LocalFunction()
            {
                return null;
            }
            return LocalFunction();
        }

        public ActionResult PostRequest()
        {
            [RequestSizeLimit(8_000_001)] // Noncompliant
            ActionResult LocalFunction()
            {
                return null;
            }
            return LocalFunction();
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 8_000_001)] // Noncompliant
        public ActionResult RequestFormLimitsAboveLimit()
        {
            [RequestSizeLimit(8_000_001)] // Secondary [1]
            [RequestFormLimits(MultipartBodyLengthLimit = 8_000_001)] // Noncompliant [1]
            ActionResult LocalFunction()
            {
                return null;
            }
            return LocalFunction();
        }

        [HttpPost]
        public ActionResult RequestFormLimitsBelowLimit()
        {
            [DisableRequestSizeLimit()] // Noncompliant
            ActionResult LocalFunction()
            {
                return null;
            }
            return LocalFunction();
        }
    }
}
