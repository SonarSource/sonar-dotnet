using System;
using System.Collections.Generic;

class Fruit
{
    public object Property => null;
}

struct SomeStruct { }

class Program
{
    private object someField;

    private object LocalProperty => null;

    public void Foo(Object x)
    {
        if (x is Fruit)  // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            var f1 = (Fruit)x;
//                   ^^^^^^^^ Secondary
            var f2 = (Fruit)x;
//                   ^^^^^^^^ Secondary
        }

        var f = x as Fruit;
        if (x != null) // Compliant
        {

        }
    }

    public void IgnoreMemberAccess(Fruit arg)
    {
        var differentInstance = new Fruit();
        var f = new Fruit();

        if (arg.Property is Fruit)                  // Compliant, the cast is on a different instance
        {
            _ = (Fruit)differentInstance.Property;
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/9491
        if (arg.Property is Fruit)                  // Noncompliant
        {
            _ = ((Fruit)(arg.Property)).Property;   // Secondary
        }

        if (arg.Property is Fruit)                  // Noncompliant
        {
            _ = ((Fruit)((arg.Property))).Property; // Secondary
        }

        if (arg.Property is Fruit)                  // Noncompliant
        {
            _ = ((Fruit)arg.Property).Property;     // Secondary
        }

        if (f.Property is Fruit)                    // Compliant, the cast is on a different instance
        {
            _ = (Fruit)differentInstance.Property;
        }

        if (f.Property is Fruit)        // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            _ = (Fruit)f.Property;      // Secondary
        }

        if (LocalProperty is Fruit)     // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            _ = (Fruit)LocalProperty;   // Secondary
        }
    }

    public void Bar(object x)
    {
        if (!(x is Fruit))
        {
            var f1 = (Fruit)x; // Compliant - but will throw
        }
        else
        {
            var f2 = (Fruit)x; // Compliant - FN
        }

    }

    public void WithStructs(object x)
    {
        if (x is int)           // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            var res = (int)x;   // Secondary
        }

        if (x is SomeStruct)            // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            var res = (SomeStruct)x;    // Secondary
        }
    }

    public void IsFollowedByAs(object x) {
        if (x is Fruit)         // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            _ = x as Fruit;     // Secondary
        }

        if (x is Fruit)         // Compliant, "==" binary operator doesn't raise
        {
            _ = x == null;
        }


        if (x is SomeStruct?)       // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            _ = x as SomeStruct?;   // Secondary
        }
    }

    public void IsFollowedByIs(object x) {
        if (x is Fruit)         // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            _ = x is Fruit;     // Secondary
        }


        if (x is SomeStruct?)       // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            _ = x is SomeStruct?;   // Secondary
        }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/2314
    public void TakeIdentifierIntoAccount(object x)
    {
        if (x is Fruit)
        {
            var f = new Fruit();
            var c = (Fruit)f;
        }

        if (someField is Fruit) // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            var fruit = (Fruit)this.someField;
//                      ^^^^^^^^^^^^^^^^^^^^^ Secondary
        }

        if (someField is Fruit) // Noncompliant
        {
            var fruit = ((Fruit)(this.someField));
//                       ^^^^^^^^^^^^^^^^^^^^^^^ Secondary
        }
    }

    public void UnknownFoo(object x)
    {                                   // Error@+1 [CS0246]
        if (x is UndefinedType)         // Noncompliant
        {                               // Error@+1 [CS0246]
            var c = (UndefinedType)x;   // Secondary
        }
    }
}

public class Bar<T> { }

public class Foo<T>
{
    public void Process(object message)
    {
        if (message is Bar<T>/*comment*/)     // Noncompliant {{Replace this type-check-and-cast sequence to use pattern matching.}}
        {
            var sub = (Bar<T>/**/) message;   // Secondary
        }
    }
}
