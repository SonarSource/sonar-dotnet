using System;
using System.Collections.Generic;

class Fruit { }
struct SomeStruct { }

class Program
{
    private object someField;

    public void Foo(Object x)
    {
        if (x is Fruit)  // Noncompliant
        {
            var f1 = (Fruit)x;
//                   ^^^^^^^^ Secondary
        }

        var f = x as Fruit;
        if (x != null) // Compliant
        {

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
            var f2 = (Fruit)x; // Compliant - should be non compliant
        }

    }

    public void WithStructs(object x)
    {
        if (x is int)           // Noncompliant
        {
            var res = (int)x;   // Secondary
        }

        if (x is SomeStruct)            // Noncompliant
        {
            var res = (SomeStruct)x;    // Secondary
        }
    }

    public void IsFollowedByAs(object x) {
        if (x is Fruit)         // Noncompliant
        {
            _ = x as Fruit;     // Secondary
        }

        if (x is Fruit)         // Compliant, "==" binary operator doesn't raise
        {
            _ = x == null;
        }


        if (x is SomeStruct?)       // Noncompliant
        {
            _ = x as SomeStruct?;   // Secondary
        }
    }

    public void IsFollowedByIs(object x) {
        if (x is Fruit)         // Noncompliant
        {
            _ = x is Fruit;     // Secondary
        }


        if (x is SomeStruct?)       // Noncompliant
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

        if (someField is Fruit) // Noncompliant
        {
            var fruit = (Fruit)this.someField;
//                      ^^^^^^^^^^^^^^^^^^^^^ Secondary
        }
    }

    public void UnknownFoo(object x)
    {                                   // Error@+1 [CS0246]
        if (x is UndefinedType)         // Noncompliant
        {                               // Error@+1 [CS0246]
            var c = (UndefinedType)x;   // Secondary
        }
    }

    public void MultipleCasts_RootBlock(object arg)
    {
        _ = (Fruit)arg; // Noncompliant {{Extract this repetitive cast into a variable.}}
        //  ^^^^^^^^^^
        _ = (Fruit)arg;             // Secondary
        _ = (Fruit/*Comment*/)arg;  // Secondary
        _ = (  Fruit   )arg;        // Secondary with extra whitespace
    }

    public void MultipleCasts_SameBlock(object arg)
    {
        if (true)
        {
            _ = (Fruit)arg; // Noncompliant {{Extract this repetitive cast into a variable.}}
            _ = (Fruit)arg; // Secondary
            _ = (Fruit)arg; // Secondary
        }
    }

    public void MultipleCasts_NestedBlock(object arg)
    {
        _ = (Fruit)arg;         // Compliant, not in the same block
        if (true)
        {
            _ = (Fruit)arg;     // Noncompliant [Outer] {{Extract this repetitive cast into a variable.}}
            foreach (var ch in "Lorem ipsum")
            {
                _ = (Fruit)arg; // Noncompliant [Inner]
                _ = (Fruit)arg; // Secondary    [Inner]
            }
            _ = (Fruit)arg;     // Secondary    [Outer]
        }
    }

    public void MultipleCasts_Lambda(object arg)
    {
        _ = (Fruit)arg; // Compliant, not in the same block
        Action a = () =>
        {
            _ = (Fruit)arg; // Noncompliant [Action]
            _ = (Fruit)arg; // Secondary    [Action]
        };
        Func<object, int> f = x =>
        {
            _ = (Fruit)arg;
            _ = (Fruit)x;
            return 0;
        };
    }

    public void MultipleCastsDifferentBlocks(object arg)
    {
        if (true)
        {
            _ = (Fruit)arg; // Compliant, we only look into the current and nested blocks
        }

        while(false)
        {
            _ = (Fruit)arg;
        }

        _ = (Fruit)arg;

        switch (42)
        {
            case 1:
                _ = (Fruit)arg; // Compliant, not the same block
                break;
            case 2:
                _ = (Fruit)arg;
                break;
        }
    }
}

public class Bar<T> { }

public class Foo<T>
{
    public void Process(object message)
    {
        if (message is Bar<T>/*comment*/)     // Noncompliant
        {
            var sub = (Bar<T>/**/) message;   // Secondary
        }
    }
}
