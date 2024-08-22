using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
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

public class StreamsBase
{
    protected const long BaseToLarge = 1024 * 1024 * 8;
    protected const long BaseCompliant = 1024;
}

public class StreamsDerived : StreamsBase
{
    private const long ExceedsLimit = 1024 * 1024 * 8;
    private const long Compliant = 1024;

    public static void OpenReadStream(IBrowserFile file, long unknownSize)
    {
        const long maxFileSizeNonCompliant = 1024 * 1024 * 20;
        file.OpenReadStream(maxFileSizeNonCompliant); // FN
        file.OpenReadStream(ExceedsLimit); // FN
        file.OpenReadStream(20);
        file.OpenReadStream(Compliant);
        file.OpenReadStream(BaseToLarge); // FN
        file.OpenReadStream(BaseCompliant);
        file.OpenReadStream(unknownSize);

        var x = unknownSize;
        file.OpenReadStream(x); // We do not track the value of x
        file.OpenReadStream(GetSize()); // We do not track the value of GetSize()

        long[] sizes = [ExceedsLimit, Compliant];
        file.OpenReadStream(sizes[0]); // FN we don't track the value of sizes[0]
        file.OpenReadStream(sizes[1]);
        file.OpenReadStream(sizes[2]);

        if (unknownSize > 1024 * 1024 * 20)
        {
            file.OpenReadStream(unknownSize); // FN we don't track the value of unknownSize
        }
        else
        {
            file.OpenReadStream(unknownSize);
        }

        static long GetSize() => 1024 * 1024 * 20;
    }

    public static void GetMultipleFiles(InputFileChangeEventArgs inputFileChange)
    {
        foreach (var part in inputFileChange.GetMultipleFiles()) // Default to a maximum of 10 files
        {
            part.OpenReadStream(); // Defaults to a maximum of 500 KB, 10 files x 500 KB = 5 MB which is less than the default 8 MB limit
        }

        foreach (var part in inputFileChange.GetMultipleFiles()) // Default to a maximum of 10 files
        {
            part.OpenReadStream(1024); // 10 files x 1 KB = 10 KB which is less than the default 8 MB limit
            part.OpenReadStream(1024 * 1024); // FN 10 files x 1 MB = 10 MB which exceeds the default 8 MB limit
            part.OpenReadStream(ExceedsLimit); // FN
        }

        foreach (var part in inputFileChange.GetMultipleFiles(1))
        {
            part.OpenReadStream(1024); // 1 files x 1 KB = 1 KB which is less than the default 8 MB limit
            part.OpenReadStream(1024 * 1024);
            part.OpenReadStream(ExceedsLimit); // FN
        }
    }

    public static void GetMultipleFiles_ArrayAccess(InputFileChangeEventArgs inputFileChange)
    {
        var differentIndexes = inputFileChange.GetMultipleFiles(20);
        differentIndexes[0].OpenReadStream(2 * 1024 * 1024);
        differentIndexes[1].OpenReadStream(3 * 1024 * 1024);
        differentIndexes[2].OpenReadStream(4 * 1024 * 1024); // FN

        var sameIndex = inputFileChange.GetMultipleFiles(20);
        sameIndex[0].OpenReadStream(2 * 1024 * 1024);
        sameIndex[0].OpenReadStream(3 * 1024 * 1024);
        sameIndex[0].OpenReadStream(4 * 1024 * 1024); // FN

        // Testing a mix of method calls for the same index and different indexes
        var multipleAccess = inputFileChange.GetMultipleFiles(20);
        multipleAccess[0].OpenReadStream(2 * 1024 * 1024);
        multipleAccess[0].OpenReadStream(3 * 1024 * 1024);
        multipleAccess[1].OpenReadStream(4 * 1024 * 1024); // FN
    }

    public static void GetMultipleFiles_InLambda(InputFileChangeEventArgs inputFileChange)
    {
        Parallel.ForEach(inputFileChange.GetMultipleFiles(9), file =>
        {
            file.OpenReadStream(1024 * 1024); // FN
        });
    }
}
