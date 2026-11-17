using System;
using System.Collections;
using System.Collections.Generic;

public class Cat;
public class Dog;

public union Pet(Cat, Dog)
{
    public string Describe() =>
        this switch // Compliant NET-4907
        {
            Cat => "cat",
            Dog => "dog",
        };

    public bool IsCat() => this is Cat; // Compliant NET-4907

    public bool IsDog() => (this) is Dog; // Compliant
    public bool IsEmptyCat() => this is Cat { }; // Compliant
    public bool IsCatOrDog() => this is Cat or Dog; // Compliant

    public string Name()
    {
        switch (this) // Compliant
        {
            case Cat:
                return "cat";
            default:
                return "dog";
        }
    }

    public class Nested
    {
        public bool IsNestedChild() => this is NestedChild; // Noncompliant
    }

    public class NestedChild : Nested;
}

public class Owner
{
    public union Inner(Cat, Dog)
    {
        public bool IsCat() => this is Cat; // Compliant
    }

    public bool IsOwnerChild() => this is OwnerChild; // Noncompliant
}

public class OwnerChild : Owner;

public partial union Animal(Cat, Dog);

public partial union Animal
{
    public bool IsCat() => this is Cat; // Compliant
}

[System.Runtime.CompilerServices.Union]
public struct CustomPet : System.Runtime.CompilerServices.IUnion
{
    public CustomPet(Cat value) => Value = value;
    public CustomPet(Dog value) => Value = value;
    public object Value { get; }

    public string Describe() =>
        this switch // Compliant NET-4907
        {
            Cat => "cat",
            Dog => "dog",
            _ => "none",
        };

    public bool IsCat() => this is Cat; // Compliant NET-4907

    public string Name()
    {
        switch (this) // Compliant NET-4907
        {
            case Cat:
                return "cat";
            default:
                return "dog";
        }
    }
}

[System.Runtime.CompilerServices.Union]
public class CustomPetClass : System.Runtime.CompilerServices.IUnion
{
    public CustomPetClass(Cat value) => Value = value;
    public CustomPetClass(Dog value) => Value = value;
    public object Value { get; }

    public bool IsCat() => this is Cat; // Compliant NET-4907
}

public struct NotAUnion : System.Runtime.CompilerServices.IUnion
{
    public NotAUnion(Cat value) => Value = value;
    public object Value { get; }

    public bool IsComparable() => this is System.IComparable; // Noncompliant
}

public struct Point
{
    public bool IsComparable() => this is System.IComparable; // Noncompliant
}

public closed class Shape
{
    public string Describe() =>
        this switch // Noncompliant
        {
            Circle => "circle", // Secondary
            Square => "square", // Secondary
        };

    public bool IsCircle() => this is Circle; // Noncompliant

    public bool IsSquare() => (this) is Square; // Noncompliant
    public bool IsEmptyCircle() => this is Circle { }; // Noncompliant
    public bool IsCircleOrSquare() => this is Circle or Square; // Noncompliant

    public string Name()
    {
        switch (this) // Noncompliant
        {
            case Circle: // Secondary
                return "circle";
            default:
                return "square";
        }
    }
}

public class Circle : Shape;
public class Square : Shape;

namespace Tests.Diagnostics
{
    class Program
    {
        public int MyProperty { get; set; }
        public object MyProperty2 { get; set; }

        public Program(object o, object i)
        {
            if (this is Program and { MyProperty: 2 }) // Noncompliant
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
            }

            if (this is not IDisposable) // Noncompliant {{Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^
            {
            }

            if (o is not IDisposable)
            {
            }

            if (o is Program)
            {
            }

            if (this is null) // Compliant as it does not check the type.
            {

            }

            if (this is not null) // Compliant as it does not check the type.
            {

            }

            if (this is Program or IDisposable) // Noncompliant
            {
            }

            if (this is Program and IDisposable) // Noncompliant
            {
            }

            if (this is not null and (IDisposable or Program))  // Noncompliant
            {
            }

            if (o is not null and (IDisposable or Program))
            {
            }

            if (this is var variable) // Compliant
            {
            }

            if (this is { MyProperty: 5 }) // Compliant as it does not check the type
            {
            }

            if (this is ({ MyProperty: 5 })) // Compliant as it does not check the type
            {
            }

            if (this is not { MyProperty: 5 })
            {
            }

            if (this is { MyProperty: 5 })
            {
                if (this is Program) // Noncompliant
                {
                }
            }

            if (this is Program p) // Noncompliant
            {
            }

            if (this is { MyProperty: int prop1 and > 5 }) // Compliant
            {
            }

            if (this is Program { MyProperty: int prop2 and > 5 } e) // Noncompliant
            {
            }

            if (this is { MyProperty2: IDisposable }) // Compliant
            {
            }

            switch (this) // Compliant
            {
                case { MyProperty: 5 }:
                    break;
                case { MyProperty: 3 }:
                case null:
                    break;
                case not null:
                    break;
            }

            switch (this) // Noncompliant Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.
//                  ^^^^
            {
                case IDisposable: // Secondary
//                   ^^^^^^^^^^^
                    break;
                default:
                    break;
            }

            switch (this) // Noncompliant
            {
                case IEnumerable enumerable: // Secondary
//                   ^^^^^^^^^^^^^^^^^^^^^^
                    break;
                default:
                    break;
            }

            switch (this) // Noncompliant
            {
                case Program { MyProperty: 5 }: // Secondary
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^
                    break;
                default:
                    break;
            }

            switch (o)
            {
                case IDisposable:
                    break;
                case IEnumerable:
                    break;
                default:
                    break;
            }

            var result = (((this))) switch // Noncompliant Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.
//                       ^^^^^^^^^^
            {
                IDisposable => 1, // Secondary
//              ^^^^^^^^^^^
                IEnumerable => 2, // Secondary
//              ^^^^^^^^^^^
                _ => 3
            };

            result = this switch // Noncompliant
//                   ^^^^
            {
                IDisposable disposable => 1, // Secondary
//              ^^^^^^^^^^^^^^^^^^^^^^
                _ => 3
            };

            result = this switch // Noncompliant
            {
                Program { MyProperty: 5 } => 1, // Secondary
//              ^^^^^^^^^^^^^^^^^^^^^^^^^
                _ => 3
            };

            result = this switch // Compliant
            {
                { MyProperty: 5 } => 1,
                { MyProperty: 3 } => 2,
                null => 4,
                not null => 5
            };

            result = o switch
            {
                IDisposable => 1,
                IEnumerable => 2,
                _ => 3
            };

            if ((o, (this, i)) is (Program, (Program, Program))) // FN
            {
            }

            switch ((this, o, i)) // FN
            {
                case (Program, Program, Program):
                    break;
                default:
                    break;
            }
        }

        public bool SomeMethod(object o)
        {
            if (this is { MyProperty: 5 })
            {
                return this is Program { MyProperty: 5 }; // Noncompliant
            }

            switch (this)
            {
                case { MyProperty: 5 }:
                    return this is Program { MyProperty: 5 }; // Noncompliant
                default:
                    break;
            }

            switch (o)
            {
                case IDisposable:
                    {
                        return this is Program { MyProperty: 5 }; // Noncompliant
                    }
                case IEnumerable:
                    break;
                default:
                    break;
            }

            var result = this switch
            {
                { MyProperty: 5 } => this is Program { MyProperty: 5 }, // Noncompliant
//                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                { MyProperty: 3 } => this is Program { MyProperty: 3 }, // Noncompliant
                _ => false
            };

            return false;
        }
    }

    record R
    {
        void Foo()
        {
            if (this is IDisposable) // Noncompliant {{Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.}}
//              ^^^^^^^^^^^^^^^^^^^
            {
            }
        }
    }
}

namespace Tests.Diagnostics.ExtendedPropertyPatterns
{
    class Program
    {
        public List<int> MyProperty { get; set; }

        public Program(object o, object i)
        {
            if (this is Program { MyProperty.Count: 2 }) // Noncompliant
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
            }

            if (o is Program and { MyProperty.Count: 2 })
            {
            }

            switch (this) // Noncompliant
            {
                case Program { MyProperty.Count: 5 }: // Secondary
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    break;
                default:
                    break;
            }

            var result = this switch // Noncompliant
            {
                Program { MyProperty.Count: 5 } => 1, // Secondary
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                _ => 3
            };
        }
    }
}

namespace Tests.Diagnostics
{
    public class SomeClass : ArrayList
    {
        public void SomeMethod()
        {
            var a = this is [1, 2, 3]; // Noncompliant

            switch (this) // Noncompliant, switch statement
            {
                case [1, 2, 3]: // Secondary
                    return;
                case [4, 5, 6]: // Secondary
                    return;
                default:
                    break;
            }

            var res = this switch // Noncompliant, switch expression
            {
                [1, 2, 3] => 1, // Secondary
                [4, 5, 6] => 2, // Secondary
                _ => 42
            };
        }
    }
}
