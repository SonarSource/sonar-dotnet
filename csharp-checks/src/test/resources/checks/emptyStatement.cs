class Program
{
    static void Main(string[] args)
    {
        doSomething();                                // Compliant
        doSomethingElse();;                           // Nonompliant
        ;                                             // Noncompliant

        for (int i = 1; i <= 10; doSomething(), i++); // Noncompliant
    }
}
