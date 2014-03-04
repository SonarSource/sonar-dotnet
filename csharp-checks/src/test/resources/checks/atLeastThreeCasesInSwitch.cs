using System;

class Program
{
    static void Main(string[] args)
    {
      switch (a) {           // Noncompliant
        case 0:
          doSomething();
          break;
        default:
          doSomethingElse();
          break;
      }

      switch (variable) {    // Noncompliant
      }

      switch (a) {           // Compliant
        case 0:
          doSomething();
          break;
        case 0:
        default:
          doSomethingElse();
          break;
      }
    }
}
