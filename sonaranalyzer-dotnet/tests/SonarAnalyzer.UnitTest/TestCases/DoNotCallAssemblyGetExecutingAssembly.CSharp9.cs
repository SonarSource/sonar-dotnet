using System.Reflection;

Assembly assem = Assembly.GetExecutingAssembly(); // Noncompliant - FP, there is not alternative for top-level statements

assem = typeof(Assembly).Assembly; // Compliant

record R
{
    public void Foo()
    {
        Assembly assem = Assembly.GetExecutingAssembly(); // Noncompliant
        assem = typeof(R).Assembly; // Compliant
    }
}
