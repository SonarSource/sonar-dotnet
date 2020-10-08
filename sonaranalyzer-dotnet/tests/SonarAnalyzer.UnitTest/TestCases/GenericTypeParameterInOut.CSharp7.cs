using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    // https://github.com/SonarSource/sonar-dotnet/issues/1513
    public interface IFoo1<T> // Compliant, T cannot be in or out when used in a tuple
    {
        (T, object) Process(T item);
    }
    public interface IFoo2<T> // Compliant, T cannot be in or out when used in a tuple
    {
        void Process((T, object) item);
    }
    public interface IFoo3<T> // Noncompliant {{Add the 'in' keyword to parameter 'T' to make it 'contravariant'.}}
    {
        void Process(T item);
    }
    public interface IFoo4<T> // Noncompliant {{Add the 'out' keyword to parameter 'T' to make it 'covariant'.}}
    {
        T Process(object item);
    }
}
