using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    // See https://github.com/SonarSource/sonar-dotnet/issues/4954
    internal class FlowCaptureOperation
    {
        internal string Special = "";  // Compliant FN

        internal FlowCaptureOperation(string foo)
        {
            Special = foo != "" ? foo : "foo";
        }
    }
}
