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
