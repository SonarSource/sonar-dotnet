using System;

public class Program
{
    void Benchmark()
    {
        var start = DateTime.Now;
        MethodToBeBenchmarked();
        Console.WriteLine($"{(DateTime.Now - start).TotalMilliseconds} ms"); // Noncompliant

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
            MethodToBeBenchmarked();
            time = DateTime.Now - start; // Noncompliant
        }
    }

    void MethodToBeBenchmarked()
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
}
