using System;

public class Program
{
    void Benchmark()
    {
        var start = DateTime.Now;
        // Some method
        Console.WriteLine($"{(DateTime.Now - start).TotalMilliseconds} ms"); // Noncompliant {{Avoid using "DateTime.Now" for benchmarking or timespan calculation operations.}}
        //                    ^^^^^^^^^^^^^^^^^^^^

        start = DateTime.Now;
        // Some method
        Console.WriteLine($"{DateTime.Now.Subtract(start).TotalMilliseconds} ms"); // Noncompliant
        //                   ^^^^^^^^^^^^^^^^^^^^^
    }

    private const int MinRefreshInterval = 100;

    void Timing()
    {
        var lastRefresh = DateTime.Now;
        if ((DateTime.Now - lastRefresh).TotalMilliseconds > MinRefreshInterval) // Noncompliant
        {
            lastRefresh = DateTime.Now;
            // Refresh
        }
    }

    void Combinations(TimeSpan timeSpan, DateTime dateTime)
    {
        _ = (DateTime.Now - dateTime).Milliseconds; // Noncompliant
        _ = DateTime.Now.Subtract(dateTime).Milliseconds; // Noncompliant

        _ = (DateTime.Now - TimeSpan.FromSeconds(1)).Millisecond; // Compliant
        _ = DateTime.Now.Subtract(TimeSpan.FromDays(1)).Millisecond; // Compliant

        _ = (DateTime.Now - timeSpan).Millisecond; // Compliant
        _ = DateTime.Now.Subtract(timeSpan).Millisecond; // Compliant

        _ = (DateTime.UtcNow - dateTime).Milliseconds; // Compliant
        _ = DateTime.UtcNow.Subtract(dateTime).Milliseconds; // Compliant

        _ = (new DateTime(1) - dateTime).Milliseconds; // Compliant
        _ = new DateTime(1).Subtract(dateTime).Milliseconds; // Compliant

        void LocalMethod()
        {
            var sec = DateTime.Now;
            // something
            Console.WriteLine($"{(DateTime.Now - sec).TotalMilliseconds} ms"); // Noncompliant
        }
    }

    private TimeSpan time;

    public TimeSpan Time
    {
        get => time;
        set
        {
            var start = DateTime.Now;
            time = DateTime.Now - start; // Noncompliant
        }
    }

    void SwitchExpression(DateTime start)
    {
        var a = 1;
        switch (a)
        {
            case 1:
                time = DateTime.Now - start - new TimeSpan(1); // Noncompliant
                break;
        }
    }

    void NonInLineDateTimeNow()
    {
        var start = DateTime.Now;
        // Some method
        var end = DateTime.Now;
        var elapsedTime = end - start; // FN
    }

    void EdgeCases(DateTime dateTime, TimeSpan timeSpan)
    {
        (true ? DateTime.Now : new DateTime(1)).Subtract(dateTime); // FN
        (true ? DateTime.Now : new DateTime(1)).Subtract(timeSpan); // Compliant

        DateTime.Now.AddDays(1).Subtract(dateTime); // FN
        DateTime.Now.Subtract(); // Error [CS1501]
    }
}

public class FakeDateTimeSubtract
{
    void MyMethod(System.DateTime dateTime)
    {
        _ = DateTime.Now.Subtract(dateTime).Milliseconds; // Compliant
    }

    public static class DateTime
    {
        public static System.DateTime Now { get; }
    }
}
