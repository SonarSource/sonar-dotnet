using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.RequestsWithExcessiveLength.GenericAttributes;

public class MyController : Controller
{
    [HttpPost]
    [DisableRequestSizeLimit] // Noncompliant {{Make sure the content length limit is safe here.}}
    public ActionResult PostRequestWithNoLimit()
    {
        return null;
    }

    [HttpPost]
    [RequestSizeLimit(8_388_609)] // Noncompliant
    public ActionResult RequestSizeLimitAboveLimit()
    {
        [RequestFormLimits(MultipartBodyLengthLimit = 8_388_609)] // Noncompliant
        ActionResult LocalFunction()
        {
            return null;
        }
        return LocalFunction();
    }

    public ActionResult PostRequest()
    {
        [RequestSizeLimit(8_388_609)] // Noncompliant
        ActionResult LocalFunction()
        {
            return null;
        }
        return LocalFunction();
    }

    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = 8_388_609)] // Noncompliant
    public ActionResult RequestFormLimitsAboveLimit()
    {
        [RequestSizeLimit(8_388_609)] // Secondary [1]
        [RequestFormLimits(MultipartBodyLengthLimit = 8_388_609)] // Noncompliant [1]
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

internal class TestCases
{
    [RequestSizeLimit(8_388_609)] // Noncompliant
    public void Foo() { }

    [GenericAttribute<int>(8_388_609)] // FN for performance reasons we decided not to handle derived classes
    public void Bar() { }

    public void Bar(IEnumerable<int> collection)
    {
        [RequestSizeLimit(8_388_609)] int Get() => 1; // Noncompliant [flow1] - FP not a security problem since the local methods and lambdas are not reachable
        [RequestSizeLimit(1)] int GetSmall() => 1;

        _ = collection.Select([DisableRequestSizeLimitAttribute()] (x) => x + 1); // Noncompliant [flow2] - FP

        Action a = [RequestSizeLimit(8_388_609)] () => { }; // Secondary [flow3]

        Action x = true
            ? ([RequestFormLimits(MultipartBodyLengthLimit = 8_388_609)] () => { })
            : [RequestFormLimits(MultipartBodyLengthLimit = 7_000_001)] () => { };

        Call([RequestFormLimits(MultipartBodyLengthLimit = 8_388_609)] (x) => { }); // Noncompliant [flow3] {{Make sure the content length limit is safe here.}} - FP
    }

    private void Call(Action<int> action) => action(1);
}

public class GenericAttribute<T> : RequestSizeLimitAttribute
{
    public GenericAttribute(long bytes) : base(bytes) { }
}
