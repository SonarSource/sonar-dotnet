class C1
{
    void f1()
    {
        for (int i = 0; i < 42; i++){}  // Noncompliant

        switch (a)                      // Noncompliant
        {
        }

        try                             // Noncompliant
        {
        } catch (Exception e)           // Compliant
        {
          // Ignore
        } finally                       // Noncompliant
        {
        }

        for (int i = 0; i < 42; i++);   // Compliant

        if (a)                          // Compliant
        {
          // Do nothing because of X and Y
        }

        switch (a)                     // Compliant
        {
          case 1:
            break;
          default:
            break;
        }
    }

    void f2()                           // Compliant
    {
    }
}

class C1
{                                       // Compliant
}
