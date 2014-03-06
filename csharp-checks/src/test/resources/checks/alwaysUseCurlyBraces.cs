class C
{
    void f()
    {
        if (condition) doSomething();               // Noncompliant

        for (int i = 0; i < 10; i++) doSomething(); // Noncompliant

        foreach (int i in myArray) doSomething();   // Noncompliant

        while (condition) doSomething();            // Noncompliant

        do something(); while (condition);          // Noncompliant

        if (condition) {                            // Compliant
        } else doSomething();                       // Noncompliant

        if (condition)                              // Noncompliant
          if (condition) {
          }

        if (condition) {                            // Compliant
        }

        if (condition) {                            // Compliant
        } else if (condition) {                     // Compliant
        }

        for (int i = 0; i < 10; i++) {              // Compliant
        }

        while (condition) {                         // Compliant
        }

        do {                                        // Compliant
          something();
        } while (condition);
    }
}
