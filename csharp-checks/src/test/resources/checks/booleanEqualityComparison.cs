using System;

class Program
{
    static void Main(string[] args)
    {
        !true;                      // Noncompliant
        !false;                     // Noncompliant
        (a == false) && true;       // Noncompliant
        a == true;                  // Noncompliant
        a != false;                 // Noncompliant
        a != true;                  // Noncompliant
        false == a;                 // Noncompliant
        true == a;                  // Noncompliant
        false != a;                 // Noncompliant
        true != a;                  // Noncompliant
        false && foo();             // Noncompliant
        foo() || true;              // Noncompliant

        a == foo(true);             // Compliant
        true < 0;                   // Compliant
        ~true;                      // Compliant
        ++ true;                    // Compliant
        !foo;                       // Compliant
        foo() && bar();             // Compliant
        a == true == b;             // Compliant

    }
}
