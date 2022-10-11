using System;

namespace Tests.Diagnostics
{
    public class GenericAttribute<T> : Attribute { } // Noncompliant

    [AttributeUsage(AttributeTargets.Class)]
    public class MyCompliantAttribute<T> : Attribute { }
}
