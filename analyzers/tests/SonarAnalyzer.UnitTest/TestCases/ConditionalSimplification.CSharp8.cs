﻿public class TestCases
{
    public void SameBaseType(bool condition)
    {
        object x;
        X o = null;
        if (o == null) // Compliant, target type conditionals are supported only from C# 9
        {
            x = new Y();
        }
        else
        {
            x = o;
        }

        o = o ?? new X(); // Noncompliant {{Use the '??=' operator here.}}

        Base elem;
        if (condition) // Compliant, target type conditionals are supported only from C# 9
        {
            elem = new A();
        }
        else
        {
            elem = new B();
        }
    }

    class X { }
    class Y { }
    class Base { }
    class A : Base { }
    class B : Base { }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4607
public class Example
{
    public string Foo { get; set; }
    public string Bar { get; set; }

    public Example Fallback(Example other)
    {
        return new Example
        {
            Foo = Foo ?? other.Foo, // Noncompliant FP
            Bar = Bar ?? other.Bar // Noncompliant FP
        };
    }
}
