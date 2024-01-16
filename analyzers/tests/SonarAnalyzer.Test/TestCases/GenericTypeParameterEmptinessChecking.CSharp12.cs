using System.Collections.Generic;
using System.Linq;

public class SomeClass<T>(T arg)
{
    public bool Method()
    {
        return arg == null; // Noncompliant
    }
}
