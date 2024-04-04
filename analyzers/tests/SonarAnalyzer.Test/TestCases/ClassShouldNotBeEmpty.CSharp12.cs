using System.Collections.Generic;

public class TestCaseData(List<int> list, int expectedCount)
{
    public List<int> List { get; }
    public int ExpectedCount { get; }
}

// See https://github.com/SonarSource/sonar-dotnet/issues/9011
public class CountTestCaseData(List<int> list, int expectedCount) : TestCaseData(list, expectedCount); // Noncompliant - FP
