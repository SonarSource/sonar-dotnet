class C
{
    static void f()
    {
      if (a == b) {    // Compliant
        doSomething();
      }

      if (true == b) { // Compliant
        doSomething();
      }

      if (true) {
        doSomething();  // Noncompliant
      }

      if (false) {
        doSomething();  // Noncompliant
      }
    }
}
