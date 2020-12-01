using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class BooleanLiteralUnnecessary
    {
        public BooleanLiteralUnnecessary(bool a, bool b, bool? c)
        {
            var z = true;   // Fixed
            z = false;     // Fixed
            z = true;      // Fixed
            z = true;      // Fixed
            z = false;     // Fixed
            z = false;      // Fixed
            z = true;       // Fixed
            z = false;      // Fixed
            z = true;       // Fixed
            z = false;      // Fixed
            z = true;     // Fixed
            z = false;      // Fixed
            z = false;       // Fixed
            z = true;      // Fixed
            z = false;     // Fixed
            z = true;      // Fixed

            var x = false;                  // Fixed
            x = true;              // Fixed
            x = true;                     // Fixed
            x = (!a)                // Fixed
;                    // Fixed
            x = a;                  // Fixed
            x = a;                 // Fixed
            x = !a;                  // Fixed
            x = !a;                 // Fixed
            x = a;                  // Fixed
            x = a;                 // Fixed
            x = !a;                  // Fixed
            x = false;             // Fixed
            x = false;             // Fixed
            x = Foo();             // Fixed
            x = Foo();             // Fixed
            x = Foo();              // Fixed
            x = Foo();              // Fixed
            x = true;              // Fixed
            x = true;              // Fixed
            x = a == b;             // Fixed

            x = a == Foo(((true)));             // Compliant
            x = !a;                         // Compliant
            x = Foo() && Bar();             // Compliant

            var condition = false;
            var exp = true;
            var exp2 = true;

            var booleanVariable = condition || exp; // Fixed
            booleanVariable = !condition && exp; // Fixed
            booleanVariable = !condition || exp; // Fixed
            booleanVariable = condition && exp; // Fixed
            booleanVariable = condition; // Fixed
            booleanVariable = condition ? true : true; // Compliant, this triggers another issue S2758

            booleanVariable = condition ? exp : exp2;

            b = !(x || booleanVariable); // Fixed

            SomeFunc(true); // Fixed

            if (c == true) //Compliant
            {

            }

            var d = true ? c : false;
        }

        public static void SomeFunc(bool x) { }

        private bool Foo()
        {
            return false;
        }

        private bool Foo(bool a)
        {
            return a;
        }

        private bool Bar()
        {
            return false;
        }

        private void M()
        {
            for (int i = 0; ; i++) // Fixed
            {
            }
            for (int i = 0; false; i++)
            {
            }
            for (int i = 0; ; i++)
            {
            }

            var b = true;
            for (int i = 0; b; i++)
            {
            }
        }
    }

    public class SocketContainer
    {
        private IEnumerable<Socket> sockets;
        public bool IsValid =>
            sockets.All(x => x.IsStateValid == true); // Compliant, this failed when we compile with Roslyn 1.x
    }

    public class Socket
    {
        public bool? IsStateValid { get; set; }
    }
}
