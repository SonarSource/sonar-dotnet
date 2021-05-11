using System;

public interface IFoo
{
    void Method();
    int Property { get; set; }

    event EventHandler Event;
}

public record Foo : IFoo
{
    void IFoo.Method() // Noncompliant {{Make 'Foo' sealed, change to a non-explicit declaration or provide a new method exposing the functionality of 'IFoo.Method'.}}
    {
    }

    void Method() { }

    int IFoo.Property // Noncompliant {{Make 'Foo' sealed, change to a non-explicit declaration or provide a new method exposing the functionality of 'IFoo.Property'.}}
    { get; set; }

    int Property { get; set; }

    event EventHandler IFoo.Event // Noncompliant
    { add { } remove { } }

    // We cannot add this line yet, it crashes Roslyn with StackOverflowException https://github.com/dotnet/roslyn/issues/49286
    // event EventHandler Event {  add { } remove { } }
}

public record Bar(string Name) : IFoo
{
    void IFoo.Method() { } // Noncompliant {{Make 'Bar' sealed, change to a non-explicit declaration or provide a new method exposing the functionality of 'IFoo.Method'.}}
    int IFoo.Property // Noncompliant {{Make 'Bar' sealed, change to a non-explicit declaration or provide a new method exposing the functionality of 'IFoo.Property'.}}
    { get; set; }
    event EventHandler IFoo.Event // Noncompliant
    { add { } remove { } }
}
