using System;
using System.Collections.Generic;
using System.Linq;

bool IsDefault<T>(T value)
{
    return value == null; // Noncompliant
}

bool IsDefault2<T>(T value)
{
    return object.Equals(value, default(T)); // Compliant
}

// https://github.com/SonarSource/sonar-dotnet/issues/9493
bool NotNullWithNullableType<T>(Func<T?> arg) where T : notnull
{
    T? t = arg();
    return t == null; // Noncompliant - FP
}

public record Record<T>
{
    public bool Mx(List<T> t)
    {
        return t.Any(x => x == null); // Noncompliant
    }
}

public record R<T> where T : class
{
    public bool Foo(List<T> t)
    {
        return t.Any(x => x == null); // Compliant
    }

    public bool Bar<Q>(List<Q> t) where Q : class => t.Any(x => x == null); // Compliant
}
