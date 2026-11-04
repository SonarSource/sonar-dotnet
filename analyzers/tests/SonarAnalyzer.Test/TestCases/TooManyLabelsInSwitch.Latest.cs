using System.Threading.Tasks;
using System;

class TaskWhenEach
{
    async Task Method(int n)
    {
        Task task1 = Task.Delay(100);
        Task task2 = Task.Delay(100);
        Task task3 = Task.Delay(100);

        switch (n) // Noncompliant
        {
            case 0:
                Console.WriteLine("0");
                break;
            case 1:
                Console.WriteLine("1");
                break;
            case 2:
                Console.WriteLine("2");
                break;
            case 3:
                await foreach (Task t in Task.WhenEach(task1, task2, task3))
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Console.WriteLine("Done!");
                    }
                }
                break;
            case 4:
                Console.WriteLine("4");
                return;
        }
    }
}

class LabeledBreakAndContinue
{
    void LoopInSection(int n, int[][] grid)
    {
        switch (n) // Noncompliant
        {
            case 0:
                Console.WriteLine("0");
                break;
            case 1:
                Console.WriteLine("1");
                break;
            case 2:
                foreach (var row in grid)
                {
                    foreach (var cell in row)
                    {
                        Console.WriteLine(cell);
                    }
                }
                break;
        }
    }

    void LabeledLoopInSection(int n, int[][] grid)
    {
        switch (n) // FN NET-4967
        {
            case 0:
                Console.WriteLine("0");
                break;
            case 1:
                Console.WriteLine("1");
                break;
            case 2:
                outer: foreach (var row in grid)
                {
                    foreach (var cell in row)
                    {
                        if (cell == 0)
                        {
                            continue outer;
                        }
                        Console.WriteLine(cell);
                    }
                }
                break;
        }
    }

    void LabeledBreakInSection(int[] values)
    {
        outer: foreach (var value in values)
        {
            switch (value) // Compliant
            {
                case 0:
                    Console.WriteLine("0");
                    break outer;
                case 1:
                    Console.WriteLine("1");
                    break outer;
                case 2:
                    Console.WriteLine("2");
                    break;
            }
        }
    }

    void LabeledBreakAfterMultipleStatementsInSection(int[] values)
    {
        outer: foreach (var value in values)
        {
            switch (value) // Noncompliant
            {
                case 0:
                    Console.WriteLine("0");
                    break outer;
                case 1:
                    Console.WriteLine("1");
                    break outer;
                case 2:
                    Console.WriteLine("2");
                    Console.WriteLine("two");
                    break outer;
            }
        }
    }
}
