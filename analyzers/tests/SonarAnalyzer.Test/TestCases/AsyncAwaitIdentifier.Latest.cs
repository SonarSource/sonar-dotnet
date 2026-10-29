using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class AsyncAwaitIdentifier
    {
        public void Cs9_nuint()
        {
            nuint await = 42; // Noncompliant
        }

        private static int GetDiscount(object p) => p switch
        {
            A => 0,
            B await => 75, // Noncompliant
            _ => 25
        };
    }

    public class A { }
    public class B { }
}

namespace CSharp15
{
    public class LabeledJumps
    {
        public void Test()
        {
            async: for (var i = 0; i < 10; i++) // Noncompliant
            {
                for (var j = 0; j < 10; j++)
                {
                    if (j == 1) { continue async; }
                    if (j == 2) { break async; }
                }
            }

            await: while (true) // Noncompliant
            {
                break await;
            }
        }
    }
}
