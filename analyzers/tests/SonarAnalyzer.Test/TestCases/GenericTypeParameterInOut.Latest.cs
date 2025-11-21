using System;
using System.Collections.Generic;

// https://github.com/SonarSource/sonar-dotnet/issues/1513
public interface ISample1<T> // Compliant, T cannot be in or out when used in a tuple
{
    (T, object) Process(T item);
}
public interface ISample2<T> // Compliant, T cannot be in or out when used in a tuple
{
    void Process((T, object) item);
}
public interface ISample3<T> // Noncompliant {{Add the 'in' keyword to parameter 'T' to make it 'contravariant'.}}
{
    void Process(T item);
}
public interface ISample4<T> // Noncompliant {{Add the 'out' keyword to parameter 'T' to make it 'covariant'.}}
{
    T Process(object item);
}

public static class Extensions
{
    extension<T>(ISample3<T> sample)
    {
        T Extension(T item) => item;    // Doesn't make the interface compliant
    }

    extension<T>(ISample4<T> sample)
    {
        T Extension(T item) => item;    // Doesn't make the interface compliant
    }
}
