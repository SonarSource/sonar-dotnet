public class C
{
    private int a;

    void C()     // Compliant
    {
    }

    void f1()    // Compliant
    {
       // Do nothing because of X and Y.
    }

    void f2()     // Compliant
    {
      doSomtehing();
    }

    void f3()    // Noncompliant
    {
    }

    extern void f4();
}

abstract class AC {

  void f1()      // Compliant
  {
  }

  void f2();     // Compliant
}


