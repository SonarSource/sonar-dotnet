class Program
{
    static void Main(int a)
    {
      for (int a = 0; a < 42; a++) {
        a = 0;                                // Noncompliant
      }

      int a;
      for (a = 0; a < 42; a++) {
        a = 0;                                // Noncompliant
      }

      for (int d = 0, e = 0; d < 42; d++) {
        d = 0;                                // Noncompliant
        e = 0;                                // Noncompliant
      }

      int g;
      for (int f = 0; f < 42; f++) {
        f = 0;                                // Noncompliant
        g = 0;                                // Compliant
        for (g = 0; g < 42; g++) {
          g = 0;                              // Noncompliant
          f = 0;                              // Noncompliant
        }
        f = 0;                                // Noncompliant
        g = 0;                                // Compliant
      }

      int g = 0;                              // Compliant

      for (int h = 0; h < 42; h++) {
        h =                                   // Noncompliant
            h =                               // Noncompliant
                0;
      }

      g++;                                    // Compliant
      ++g;                                    // Compliant
      g = 0;                                  // Compliant
      doSomething(i);                         // Compliant

      for (int i = 0; 0 < 42; i++) {
        i++;                                  // Noncompliant
        --i;                                  // Noncompliant
        ++i;                                  // Noncompliant
        i--;                                  // Noncompliant
        c.i++;                                // Compliant
      }

      for (int j = 0; j < 42; j++) {          // Compliant
        for (k = 0; j++ < 42; k++) {          // Noncompliant
        }
      }

      for (int i = 0; i < 42; i++) {
        (int) i;                              // Compliant
      }

      for (int i = 0; i < 10; i++) {
        for (k = 0, Console.WriteLine(i); k < 20; i++) {  // Noncompliant
          Console.WriteLine("Hello");
          doSomething(i = 0);                 // Noncompliant
        }
      }

      for (a.i = 0; a.i < 3; a.i++) {
         a.i = 1;                             // Noncompliant
       }

       for (a.i[0] = 0; a.i[0] < 3; a.i[0]++) {
         a.i[0] = 1;                          // Noncompliant
         a.i[1] = 1;                          // Compliant
       }

      int i = 0;
      for ( ; i > 0; i++) {
       i = 1;                                 // Compliant
      }

      for (i = 0; i < 3; i++) {
              a.i = 1;                        // Compliant
       }


      for (a.i = 0; a.i < 3; a.i++) {
        a.y = 1;                              // Compliant
      }


      foreach (int element in myArray) {
       element = 0;                           // Compliant - not in check's scope
      }
    }
}
