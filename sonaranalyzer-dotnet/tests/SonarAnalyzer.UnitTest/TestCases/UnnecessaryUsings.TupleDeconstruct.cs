namespace Repro_3408_Consumer
{
    using System; // Noncompliant - FP, See: https://github.com/SonarSource/sonar-dotnet/issues/3408

    public class Repro3408
    {
        public void Consumer()
        {
            var (_, y) = Repro_3408_Provider.ServiceReturningTuples.GetPair();
        }
    }
}

namespace Repro_3408_Provider
{
    using System;

    public static class ServiceReturningTuples
    {
        public static Tuple<string, int> GetPair() => Tuple.Create("a", 1);
    }
}
