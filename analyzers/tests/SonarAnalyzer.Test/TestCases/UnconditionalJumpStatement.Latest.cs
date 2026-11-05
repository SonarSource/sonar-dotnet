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

public class LabeledBreakAndContinue
{
    public int ConditionalContinueOuterFromNestedLoop(int[][] grid)
    {
        outer: foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue outer;
                }
            }
            return row.Length; // Noncompliant {{Refactor the containing loop to do more than one iteration.}} FP NET-4841
        }
        return 0;
    }

    public int ConditionalBreakFromNestedLoop(int[][] grid)
    {
        foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    break;
                }
            }
            return row.Length; // Noncompliant
        }
        return 0;
    }

    public void ConditionalContinueOuterThenBreak(int[][] grid)
    {
        outer: foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue outer;
                }
                Console.WriteLine(cell);
                break; // FN NET-4969
            }
        }
    }

    public void ConditionalContinueThenBreak(int[][] grid)
    {
        foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue;
                }
                Console.WriteLine(cell);
                break; // Compliant
            }
        }
    }

    public void UnconditionalContinueOuterFromNestedLoop(int[][] grid)
    {
        outer: foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                Console.WriteLine(cell);
                continue outer; // Noncompliant
            }
        }
    }
}
