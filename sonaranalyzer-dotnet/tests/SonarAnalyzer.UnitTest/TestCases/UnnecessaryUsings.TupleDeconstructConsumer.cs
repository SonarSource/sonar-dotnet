using System; // Noncompliant - FP, See: https://github.com/SonarSource/sonar-dotnet/issues/3408

public class Repro3408
{
    public void Consumer()
    {
        var (_, y) = ServiceReturningTuples.GetPair();
    }
}
