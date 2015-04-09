namespace Tests.Diagnostics
{
    public class UnnecessaryBooleanLiteral
    {
        public UnnecessaryBooleanLiteral(bool a, bool b)
        {
            var x = !true;                      // Noncompliant
            x = !false;                     // Noncompliant
            x = (a == false)                // Noncompliant
                && true;                // Noncompliant
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

            x = a == Foo(true);             // Compliant
            x = !a;                         // Compliant
            x = Foo() && Bar();             // Compliant
        }

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
    }
}
