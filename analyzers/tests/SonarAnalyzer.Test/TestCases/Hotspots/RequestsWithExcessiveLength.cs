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

        [DisableRequestSizeLimitAttribute] // Noncompliant
        public ActionResult DisableRequestSizeLimitWithFullName()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(8_388_609)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult PostRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimitAttribute(8_388_609)] // Noncompliant
        public ActionResult RequestSizeLimitWithFullname()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(8_388_608)] // Compliant value is below limit.
        public ActionResult PostRequestBelowLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(8_389_632)] // Noncompliant
        public ActionResult SizeWith1024Base()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 8_388_609, MultipartHeadersLengthLimit = 42)] // Noncompliant {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult MultipartFormRequestAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimitsAttribute(MultipartBodyLengthLimit = 8_388_609)] // Noncompliant
        public ActionResult RequestFormLimitsWithFullname()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartHeadersLengthLimit = 8_388_608)]
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
        [RequestFormLimits(MultipartHeadersLengthLimit = 42, MultipartBodyLengthLimit = 8_388_609)] // Noncompliant {{Make sure the content length limit is safe here.}}
        public ActionResult RequestFormLimitsWithVariousParamsV1()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(BufferBody = true, MultipartBodyLengthLimit = 8_388_609)]  // Noncompliant {{Make sure the content length limit is safe here.}}
        public ActionResult RequestFormLimitsWithVariousParamsV2()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(BufferBody = true, MultipartHeadersLengthLimit = 42, ValueCountLimit = 42, MultipartBodyLengthLimit = 8_388_609)] // Noncompliant
        public ActionResult RequestFormLimitsWithVariousParamsV3()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(BufferBody = true, MultipartHeadersLengthLimit = 42, ValueCountLimit = 42)]
        public ActionResult RequestFormLimitsWithVariousParamsV4()
        {
            return null;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 8_388_608)] // Compliant value is below limit.
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

        [HttpPost]
        [RequestSizeLimit(int.MaxValue)] // Noncompliant
        public ActionResult RequestSizeLimitWithoutNumericLiteral()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(1000000000)] // Secondary [1] {{Make sure the content length limit is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        [RequestFormLimits(MultipartBodyLengthLimit = 1000000000)] // Noncompliant [1]
//       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public ActionResult RequestSizeLimitAndFormLimits()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(42)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1000000000)] // Noncompliant
        public ActionResult SizeBelowLimitFormsAboveLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(1000000000)] // Noncompliant
        [RequestFormLimits(MultipartBodyLengthLimit = 42)]
        public ActionResult SizeAboveLimitFormsBelowLimit()
        {
            return null;
        }

        [HttpPost]
        [RequestSizeLimit(42)]
        [RequestFormLimits(MultipartBodyLengthLimit = 42)]
        public ActionResult BothBelowLimit()
        {
            return null;
        }

        public ActionResult MethodWithoutAttributes()
        {
            return null;
        }
    }

    [DisableRequestSizeLimit] // Noncompliant
//   ^^^^^^^^^^^^^^^^^^^^^^^
    public class DisableRequestSizeLimitController : Controller
    {

    }

    [RequestSizeLimit(8_388_609)] // Noncompliant
    public class RequestSizeLimitAboveController : Controller
    {

    }

    [RequestFormLimits(MultipartBodyLengthLimit = 8_388_609)] // Noncompliant
    public class RequestFormLimitsAboveController : Controller
    {

    }

    [RequestSizeLimit(8_388_608)]
    public class RequestSizeLimitBelowController : Controller
    {

    }

    [RequestFormLimits(MultipartHeadersLengthLimit = 8_388_608)]
    public class RequestFormLimitsBelowController : Controller
    {

    }

    internal class TestCases
    {
        [DerivedAttribute(8_388_609)] // FN for performance reasons we decided not to handle derived classes
        public void Bar() { }
    }

    public class DerivedAttribute : RequestSizeLimitAttribute
    {
        public DerivedAttribute(long bytes) : base(bytes) { }
    }
}
