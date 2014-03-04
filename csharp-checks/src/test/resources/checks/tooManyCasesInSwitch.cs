class Program
{
    static void Main(string[] args)
    {
      switch (i) {   // Noncompliant - with custom value
          case 0:
          case 1:
            break;
          case 2:
            break;
          default:
            break;
      }

      switch (i) {
          case 0:
            doSomething();
          default:
            break;
      }

      switch (i) {
      }

    }
}
