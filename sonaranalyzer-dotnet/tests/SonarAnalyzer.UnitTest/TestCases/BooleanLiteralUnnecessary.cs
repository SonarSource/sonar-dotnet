namespace Tests.Diagnostics
{
    public class BooleanLiteralUnnecessary
    {
        public BooleanLiteralUnnecessary(bool a, bool b, bool? c)
        {
            var z = true || ((true));   // Noncompliant {{Remove the unnecessary Boolean literal(s).}}
//                       ^^^^^^^^^^^
            z = false || false;     // Noncompliant
            z = true || false;      // Noncompliant
            z = false || true;      // Noncompliant
            z = false && false;     // Noncompliant
            z = false && true;      // Noncompliant
            z = true && true;       // Noncompliant
            z = true && false;      // Noncompliant
            z = true == true;       // Noncompliant
            z = false == true;      // Noncompliant
            z = false == false;     // Noncompliant
            z = true == false;      // Noncompliant
            z = true != true;       // Noncompliant
            z = false != true;      // Noncompliant
            z = false != false;     // Noncompliant
            z = true != false;      // Noncompliant

            var x = !true;                  // Noncompliant
//                   ^^^^
            x = true || false;              // Noncompliant
            x = !false;                     // Noncompliant
            x = (a == false)                // Noncompliant
                && true;                    // Noncompliant
            x = a == true;                  // Noncompliant
            x = a != false;                 // Noncompliant
            x = a != true;                  // Noncompliant
            x = false == a;                 // Noncompliant
            x = true == a;                  // Noncompliant
            x = false != a;                 // Noncompliant
            x = true != a;                  // Noncompliant
            x = false && Foo();             // Noncompliant
            x = Foo() || true;              // Noncompliant
            x = a == true == b;             // Noncompliant

            x = a == Foo(((true)));             // Compliant
            x = !a;                         // Compliant
            x = Foo() && Bar();             // Compliant

            var condition = false;
            var exp = true;
            var exp2 = true;

            var booleanVariable = condition ? ((true)) : exp; // Noncompliant
//                                            ^^^^^^^^
            booleanVariable = condition ? false : exp; // Noncompliant
            booleanVariable = condition ? exp : true; // Noncompliant
            booleanVariable = condition ? exp : false; // Noncompliant
            booleanVariable = condition ? true : false; // Noncompliant
            booleanVariable = condition ? true : true; // Compliant, this triggers another issue S2758

            booleanVariable = condition ? exp : exp2;

            b = x || booleanVariable ? false : true; // Noncompliant

            SomeFunc(true || true); // Noncompliant

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

        private bool Foo(bool a      )
        {
            return a;
        }

        private bool Bar()
        {
            return false;
        }

        private void M()
        {
            for (int i = 0; true; i++) // Noncompliant
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
}
