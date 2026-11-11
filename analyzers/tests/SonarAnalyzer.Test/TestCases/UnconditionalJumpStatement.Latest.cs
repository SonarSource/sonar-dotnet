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
            return row.Length; // Compliant NET-4841
        }
        return 0;
    }

    public bool ContainsSubsequence(int[] source, int[] pattern)
    {
        search: for (var start = 0; start + pattern.Length <= source.Length; start++)
        {
            var offset = 0;
            while (offset < pattern.Length)
            {
                if (source[start + offset] != pattern[offset])
                {
                    continue search;
                }
                offset++;
            }
            return true; // Compliant NET-4841
        }
        return false;
    }

    public int UnlabeledContinueAndBreakOuterFromNestedLoop(int[][] grid)
    {
        outer: foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue;
                }
                if (cell < 0)
                {
                    break outer;
                }
            }
            return row.Length; // Noncompliant
        }
        return 0;
    }

    public int ConditionalContinueOuterWhileFromNestedLoop(int[][] grid)
    {
        var index = 0;
        outer: while (index < grid.Length)
        {
            var row = grid[index++];
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue outer;
                }
            }
            return row.Length; // Compliant
        }
        return 0;
    }

    public int UnlabeledContinueFromNestedLoopOfLabeledWhile(int[][] grid)
    {
        var index = 0;
        outer: while (index < grid.Length)
        {
            var row = grid[index++];
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue;
                }
                if (cell < 0)
                {
                    break outer;
                }
            }
            return row.Length; // Noncompliant
        }
        return 0;
    }

    public int ConditionalContinueOuterDoFromNestedLoop(int[][] grid)
    {
        var index = 0;
        outer: do
        {
            var row = grid[index++];
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue outer;
                }
            }
            return row.Length; // Compliant
        } while (index < grid.Length);
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

    public void UnconditionalBreakOuterFromNestedLoop(int[][] grid)
    {
        outer: foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                Console.WriteLine(cell);
                break outer; // Noncompliant
            }
        }
    }

    public void UnconditionalContinueOuterFromNestedDo(int[] values, bool condition)
    {
        outer: for (var i = 0; i < values.Length; i++)
        {
            do
            {
                continue outer; // Noncompliant
            } while (condition);
        }
    }

    public int ConditionalContinueMiddleFromInnermostLoop(int[][][] cube)
    {
        foreach (var plane in cube)
        {
            middle: foreach (var row in plane)
            {
                foreach (var cell in row)
                {
                    if (cell == 0)
                    {
                        continue middle;
                    }
                }
                return row.Length; // Compliant
            }
        }
        return 0;
    }

    public int ConditionalContinueOuterFromInnermostLoop(int[][][] cube)
    {
        outer: foreach (var plane in cube)
        {
            middle: foreach (var row in plane)
            {
                foreach (var cell in row)
                {
                    if (cell == 0)
                    {
                        continue outer;
                    }
                    if (cell < 0)
                    {
                        break middle;
                    }
                }
                return row.Length; // Noncompliant
            }
        }
        return 0;
    }

    public int ConditionalContinueDirectLabelOfStackedLabels(int[][] grid)
    {
        a: b: foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                if (cell == 0)
                {
                    continue b;
                }
            }
            return row.Length; // Compliant
        }
        return 0;
    }
}
