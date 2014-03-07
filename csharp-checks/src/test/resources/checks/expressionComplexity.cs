class C
{
    void f()
    {
        b = false ? (true ? (false ? (true ? 1 : 0) : 0) : 0) : 1;         // Noncompliant

        c = true || false || true || false || false;                       // Noncompliant

        d = true && false && true && false && true && true;                // Noncompliant

        call(a =>
            a = ((a ? 0 : 1) || a || true && false && true || false));     // Noncompliant

        for (i = a ? (b ? (c ? (d ? 1 : 1) : 1) : 1) : 1; i < a; i++) {}   // Noncompliant

        bool[] foo =  {                                                    // Compliant
          true && true && true && true && true,                            // Noncompliant
          true && true && true && true                                     // Compliant
        };

        e = true | false | true | false;                                   // Compliant

        a = false ? (true ? (false ? 1 : 0) : 0) : 1;                      // Compliant


        Foo foo = delegate () {                                            // Compliant
          bool a = true && true;
          bool b = true && true;
          bool c = true && true;
          bool d = true && true;
          bool e = true && true;
        };

          Foo f = new Foo {                                                // Compliant
            a = true && true,
            b = true && true,
            c = true && true,
            d = true && true,
          };

          return call(true && true && true, true && true && true);         // Compliant
    }
}
