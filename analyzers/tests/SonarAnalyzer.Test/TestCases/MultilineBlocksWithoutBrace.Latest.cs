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
