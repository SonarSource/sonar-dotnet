using System;

public class Program
{
    void Benchmark()
    {
        var start = DateTime.Now;
        MethodToBeBenchmarked();
        Console.WriteLine($"{(DateTime.Now - start).TotalMilliseconds} ms"); // Noncompliant {{Avoid using "DateTime.Now" for benchmarking or timing operations}}
        //                    ^^^^^^^^^^^^^^^^^^^^

        void LocalMethod()
        {
            var sec = DateTime.Now;
            // something
            Console.WriteLine($"{(DateTime.Now - sec).TotalMilliseconds} ms"); // Noncompliant
        }
    }

    private const int MinRefreshInterval = 100;

    void Timing()
    {
        var lastRefresh = DateTime.Now;
        if ((DateTime.Now - lastRefresh).TotalMilliseconds > MinRefreshInterval) // Noncompliant
        //   ^^^^^^^^^^^^^^^^^^^^^^^^^^
        {
            lastRefresh = DateTime.Now;
            // Refresh
        }
    }

    private TimeSpan time;

    public TimeSpan Time
    {
        get => time;
        set
        {
            var start = DateTime.Now;
            MethodToBeBenchmarked();
            time = DateTime.Now - start; // Noncompliant
        }
    }

    void SwitchExpression()
    {
        var a = 1;
        switch (a)
        {
            case 1:
                var start = DateTime.Now;
                MethodToBeBenchmarked();
                time = DateTime.Now - start - new TimeSpan(1); // Noncompliant
                break;
            case 2:
                break;
        }
    }

    bool MethodToBeBenchmarked() => true;
}
