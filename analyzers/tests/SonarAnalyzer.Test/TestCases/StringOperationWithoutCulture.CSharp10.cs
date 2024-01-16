using System;
namespace Tests.Diagnostics
{
    public class StringOperationWithoutCulture
    {
        // Repro for https://github.com/SonarSource/sonar-dotnet/issues/6439
        void TestDateTimeMethodsNet6()
        {
            var time = new TimeOnly().ToString(); // FN
            var date = new DateOnly().ToString(); // FN
        }
    }
}
