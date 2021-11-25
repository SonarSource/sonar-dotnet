using System.Reflection;

Assembly assem = Assembly.GetExecutingAssembly(); // Noncompliant - If there are other types defined in the program, calling
                                                  // 'typeof(CustomClass).Assembly' will achieve the same result.

assem = typeof(Assembly).Assembly; // Compliant

record R
{
    public void Foo()
    {
        Assembly assem = Assembly.GetExecutingAssembly(); // Noncompliant
        assem = typeof(R).Assembly; // Compliant
    }
}
