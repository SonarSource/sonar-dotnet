using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.RequestsWithExcessiveLength
{
internal class TestCases
{
    public void Bar(IEnumerable<int> collection)
    {
        [RequestSizeLimit(8_000_001)] int Get() => 1; // Noncompliant [flow1] - FP not a security problem since the local methods and lambdas are not reachable
        [RequestSizeLimit(1)] int GetSmall() => 1;

        _ = collection.Select([DisableRequestSizeLimitAttribute()] (x) => x + 1); // Noncompliant [flow2] - FP

        Action a = [RequestSizeLimit(8_000_001)] () => { }; // Secondary [flow3]

        Action x = true
                       ? ([RequestFormLimits(MultipartBodyLengthLimit = 8_000_001)] () => { })
                       : [RequestFormLimits(MultipartBodyLengthLimit = 7_000_001)] () => { };

        Call([RequestFormLimits(MultipartBodyLengthLimit = 8_000_001)] (x) => { }); // Noncompliant [flow3] {{Make sure the content length limit is safe here.}} - FP
    }

        private void Call(Action<int> action) => action(1);
    }
}
