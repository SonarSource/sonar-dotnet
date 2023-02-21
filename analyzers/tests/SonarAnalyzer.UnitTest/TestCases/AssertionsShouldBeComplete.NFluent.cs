using NFluent;

public class Program
{
    public void Invocations()
    {
        Check.That(0);      // Noncompliant {{Complete the assertion}}
        //    ^^^^
        Check.That<int>();  // Noncompliant
    }
}
