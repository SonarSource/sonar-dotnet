using System;
using System.Threading.Tasks;

class WhenEach
{
    async Task ForEach()
    {
        var one = Task.Run(() => "hey");
        var two = Task.Run(() => "there");

        await foreach (var t in Task.WhenEach(one, two))
            Console.WriteLine(t.Result);    // Secondary
            Console.WriteLine(42);          // Noncompliant
    }
}

class LabeledBreakAndContinue
{
    void LabeledContinue(int[] xs)
    {
        outer: for (var i = 0; i < xs.Length; i++)
        {
            while (i < 2)
                continue outer;        // Secondary
                Console.WriteLine(i);  // Noncompliant
        }
    }

    void LabeledBreak(int[] xs)
    {
        outer: for (var i = 0; i < xs.Length; i++)
        {
            while (i < 2)
                break outer;           // Secondary
                Console.WriteLine(i);  // Noncompliant
        }
    }

    void ForEachLabeled(int[] xs)
    {
        outer: foreach (var b in xs)
        {
            if (b > 0)
                continue outer;         // Secondary
                Console.WriteLine(b);   // Noncompliant

            if (b < 0)
            {
                break outer;
            }
        }
    }
}
