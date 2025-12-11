using System;
using System.Collections.Generic;

public class StaticLocalFunctions
{
    public void StaticLocalFunctionInLoop()
    {
        while (true)
        {
            static void Throw()
            {
                throw new InvalidOperationException("Message");
            }
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7987
class Repro7987
{
    void Test(List<object> items)
    {
        var newItems = new List<object>();
        foreach (var item in items)
        {
            LocalFunction(item);
            continue;                                   // Noncompliant FP

            void LocalFunction(object item)
            {
                if (items.Count < 10)
                    newItems.Add(item);
            }
        }
    }
}

public class NullConditionalAssignment
{
    public class Sample
    {
        public int Value { get; set; }
    }

    public void Test(Sample sample, int? value)
    {
        while (true)
        {
            sample?.Value = value ?? throw new InvalidOperationException(); // Compliant
        }
    }
}
