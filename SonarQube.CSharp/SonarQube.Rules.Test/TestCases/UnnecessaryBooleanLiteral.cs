namespace Tests.Diagnostics
{
    public class UnnecessaryBooleanLiteral
    {
        public UnnecessaryBooleanLiteral(bool a, bool b)
        {
            !true;                      // Noncompliant
            !false;                     // Noncompliant
            (a == false)                // Noncompliant
                && true;                // Noncompliant
            a == true;                  // Noncompliant
            a != false;                 // Noncompliant
            a != true;                  // Noncompliant
            false == a;                 // Noncompliant
            true == a;                  // Noncompliant
            false != a;                 // Noncompliant
            true != a;                  // Noncompliant
            false && Foo();             // Noncompliant
            Foo() || true;              // Noncompliant
            a == true == b;             // Noncompliant

            a == Foo(true);             // Compliant
            true < 0;                   // Compliant
            ~true;                      // Compliant
            ++true;                     // Compliant
            !a;                         // Compliant
            Foo() && Bar();             // Compliant
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
