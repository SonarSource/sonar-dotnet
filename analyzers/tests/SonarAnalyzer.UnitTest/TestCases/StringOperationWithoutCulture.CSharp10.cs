using System;
namespace Tests.Diagnostics
{
    public class StringOperationWithoutCulture
    {
        // Repro for https://github.com/SonarSource/sonar-dotnet/issues/6439
        void TestDateTimeMethodsNet6()
        {
            var s = string.Empty;
            s = new TimeOnly().ToString(); // FN
            s = new DateOnly().ToString(); // FN
        }
    }
}
