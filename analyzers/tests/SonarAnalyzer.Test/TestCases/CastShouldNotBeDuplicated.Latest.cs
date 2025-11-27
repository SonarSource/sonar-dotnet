using System;
using System.Collections.Generic;
using Person = (string name, string surname);

namespace Tests.Diagnostics
{
    class Fruit { public int Prop; }
    class FruitList { public List<int> Prop; }
    class Vegetable { }
    struct Water { }
    class Foo { public int x; }
    class Complex { public object x; }

    class Program
    {
        private object someField;

        public void Foo(Object x, Object y)
        {
            var fruit = x as Fruit;
            if (fruit is not Fruit) // Compliant, redundant condition, not related for the current rule
            {
            }

            object o;
            switch (x)                            // Noncompliant [switch-st-0] {{Remove this cast and use the appropriate variable.}}
            //      ^
            {
                case Fruit m:                     // Secondary [switch-st-1]
            //       ^^^^^^^
                    o = (Fruit)m;                 // Noncompliant [switch-st-1] {{Remove this redundant cast.}}
                    break;
                case Vegetable t when t != null:  // Secondary [switch-st-2]
                    o = (Vegetable)t;             // Noncompliant [switch-st-2] {{Remove this redundant cast.}}
                    break;
                case Water u:
                    o = (Water)x;                 // Secondary [switch-st-0]
                    break;
                default:
                    o = null;
                    break;
            }

            if ((x, y) is (Fruit f1, Vegetable v1))   // Secondary
            //             ^^^^^^^^
            {
                var ff1 = (Fruit)f1;                  // Noncompliant
                //        ^^^^^^^^^
            }

            if ((x, y) is (Fruit f2, Vegetable v2))   // Secondary
            {
                var ff2 = (Vegetable)v2;              // Noncompliant
            }

            if ((x, y) is (Fruit f3, Vegetable v3))   // Noncompliant
            {
                var ff3 = (Fruit)x;                   // Secondary
            }

            if ((x, y) is (Fruit f4, Vegetable v4))   // Noncompliant
            {
                var ff4 = (Vegetable)y;               // Secondary
            }

            if ((x,y) is (Fruit f5, Vegetable v5, Vegetable v51)) // Error [CS8502]
            {
                var ff5 = (Fruit)x;
            }

            if (x is Fruit f6)          // Secondary
            {
                var ff6 = (Fruit)f6;    // Noncompliant {{Remove this redundant cast.}}
                var fff6 = (Vegetable)x;
            }

            if (x is Fruit f7)          // Noncompliant {{Remove this cast and use the appropriate variable.}}
            {
                var ff7 = (Fruit)x;     // Secondary
                var fff7 = (Vegetable)x;
            }

            if (x is UnknownFruit f8)   // Error [CS0246]
            {
                var ff8 = (Fruit)x;
            }

            if (x is Water f9)
            {
                var ff9 = (Fruit)x;
            }

            x is Fruit f0; // Error [CS0201]

            if (x is not Water)
            {
                var xWater = (Water)x;
            }
            else if (x is not Fruit)
            {
                var xFruit = (Fruit)x;
            }

            var message = x switch                 // Noncompliant [switch-expression-1] {{Remove this cast and use the appropriate variable.}}
            {
                Fruit f10 =>                       // Secondary [switch-expression-2]
            //  ^^^^^^^^^
                    ((Fruit)f10).ToString(),       // Noncompliant [switch-expression-2] {{Remove this redundant cast.}}
            //       ^^^^^^^^^^
                Vegetable v11 =>                   // Secondary [switch-expression-3]
                    ((Vegetable)v11).ToString(),   // Noncompliant [switch-expression-3]
                (string left, string right) =>     // Secondary [switch-expression-4, switch-expression-5]
                    (string) left + (string) right,// Noncompliant [switch-expression-4]
                                                   // Noncompliant@-1 [switch-expression-5]
                Water w12 =>
                    ((Water)x).ToString(),         // Secondary [switch-expression-1]
                Complex { x : Fruit apple } => "apple",
                _ => "More than 10"
            };

            if ((x) is (Fruit f12, Vegetable v12))     // Noncompliant
            {
                var ff12 = (Vegetable)x;               // Secondary
            }

            Foo k = null;
            if (k is { x : 0 })
            {
            }

            if (x is (Water f13))                      // Noncompliant
            {
                var ff13 = (Water)x;                   // Secondary
            }
        }

        public void Bar(object x, object y)
        {
            if (x is not Fruit)
            {
                var f1 = (Fruit)x; // Compliant - but will throw
            }
            else
            {
                var f2 = (Fruit)x; // Compliant
            }

            if (x is Fruit { Prop: 1 } tuttyFrutty)    // Secondary [property-pattern-1]
                                                       // Noncompliant@-1 [property-pattern-2] {{Remove this cast and use the appropriate variable.}}
            {
                var aRealFruit = (Fruit)tuttyFrutty;   // Noncompliant [property-pattern-1] {{Remove this redundant cast.}}
                var anotherFruit = (Fruit)x;           // Secondary [property-pattern-2]
            }

            var foo = new Complex();
            if (foo is {x: Fruit f })                  // Secondary
            //             ^^^^^^^
            {
                var something = (Fruit)f;              // Noncompliant
                //              ^^^^^^^^
            }

            if ((x, y) is (1))                                 // Error[CS0029]
            {
            }

            if ((x, y) is (R { SomeProperty: { Count: 5 } }))  // Error [CS0246]
            {
            }
        }

        public void FooBar(object x)
        {
            if (x is nuint)         // Noncompliant
            {
                var res = (nuint)x; // Secondary
            }
        }

        public void Baz(object x, object y)
        {
            if ((x, y) is ((Fruit a, Fruit b), string v)) // Secondary
                                                          // Secondary@-1
                                                          // Secondary@-2
            {
                var a1 = (Fruit)a;                        // Noncompliant
                var b1 = (Fruit)b;                        // Noncompliant
                var v1 = (string)v;                       // Noncompliant
            }

            if (x is (Fruit or Vegetable))            // Noncompliant
            {
                var fruit = (Fruit)x;                 // Secondary
            }

            if (x is (Fruit or Vegetable))            // Noncompliant
            {
                var vegetable = (Vegetable)x;         // Secondary
            }
        }

        public void NonExistingType()
        {
            if (x is Fruit f)                       // Error [CS0103]
                                                    // Secondary@-1
            {
                var ff = (Fruit)f;                  // Noncompliant {{Remove this redundant cast.}}
            }
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2314
        public void TakeIdentifierIntoAccount(object x)
        {
            if (x is Fruit)
            {
                Fruit f = new();
                var c = (Fruit)f;
            }
        }

        public void List(Object x, Object y)
        {
            if (x is FruitList { Prop.Count: 1 } tuttyFrutty)    // Secondary [property-pattern-4]
                                                             // Noncompliant@-1 [property-pattern-3] {{Remove this cast and use the appropriate variable.}}
            {
                var aRealFruit = (FruitList)tuttyFrutty;         // Noncompliant [property-pattern-4] {{Remove this redundant cast.}}
                var anotherFruit = (FruitList)x;                 // Secondary [property-pattern-3]
            }
        }

    }
}

class MyClass
{
    void ListPattern()
    {
        object[] numbers = { 1, 2, 3 };

        if (numbers is [EmptyFruit fruit, 3, 3])     // Secondary
//                      ^^^^^^^^^^^^^^^^
        {
            var ff1 = (EmptyFruit)fruit;             // Noncompliant
        }

        if (numbers is [double number, 3, 3])   // Secondary
        {
            var ff2 = (double)number;           // Noncompliant
        }

        if (numbers is [1, 2, 3] anotherNumber)
        {
            var ff3 = (object[])anotherNumber;  // FN it will probably require a rule redesign
        }
    }
}

class EmptyFruit { }

class SomeClass
{
    private object obj;

    public void SwitchStatement(object[] array)
    {
        switch (array)
        {
            case [EmptyFruit m, 2]: // Secondary
//                ^^^^^^^^^^^^
                obj = (EmptyFruit)m; // Noncompliant
//                    ^^^^^^^^^^^^^
                break;
            default:
                obj = null;
                break;
        }
    }

    public void SwitchExpression(object[] array) =>
        obj = array switch
        {
            [EmptyFruit m, 2, 2] => // Secondary
//           ^^^^^^^^^^^^
                (EmptyFruit)m, // Noncompliant
//              ^^^^^^^^^^^^^
            _ => null
        };
}

class WithAliasAnyType
{
    void ValidCases(Person person)
    {
        _ = (Person)person;             // Compliant: not a duplicated cast
    }

    void InvalidCases(object obj)
    {
        if (obj is Person)              // Noncompliant
        {
            _ = (Person)obj;            // Secondary
        }

        if (obj is (string, string))    // FN: (string, string) and Person are equivalent
        {
            _ = (Person)obj;
        }

        if (obj is Person)              // FN: Person and (string, string) are equivalent
        {
            _ = ((string, string))obj;
        }

        if (obj is Person)              // FN: Person and (string ..., string) are equivalent
        {
            _ = ((string differentName1, string))obj;
        }

        if (obj is Person)              // FN: Person and (string, string ...) are equivalent
        {
            _ = ((string, string differentName2))obj;
        }

        if (obj is (string differentName1, string))  // FN: (string ..., string) and Person are equivalent
        {
            _ = (Person)obj;
        }

        if (obj is (string, string differentName2))  // FN: (string, string ...) and Person are equivalent
        {
            _ = (Person)obj;
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/223
class Repro_223
{
    void NumericTypes(object obj)
    {
        if (obj is int)             // Noncompliant
        {
            _ = (int)obj;           // Secondary
        }

        if (obj is double)          // Noncompliant
        {
            _ = (double)obj;        // Secondary
        }

        if (obj is ushort)          // Noncompliant
        {
            _ = (ushort)obj;        // Secondary
        }
    }

    void NullableValueTypes(object obj)
    {
        if (obj is int?)            // Noncompliant
        {
            _ = (int?)obj;          // Secondary
        }

        if (obj is byte?)           // Noncompliant
        {
            _ = (byte?)obj;         // Secondary
        }
    }

    void UsingLanguageKeywordAndFrameworkName(object obj)
    {
        if (obj is Nullable<int>)    // FN
        {
            _ = (int?)obj;
        }

        if (obj is int?)             // FN
        {
            _ = (Nullable<int>)obj;
        }

        if (obj is IntPtr)           // FN
        {
            _ = (nint)obj;
        }

        if (obj is nint)             // FN
        {
            _ = (IntPtr)obj;
        }

        if (obj is System.UIntPtr)   // FN
        {
            _ = (nuint)obj;
        }
    }

    void Enums(object obj)
    {
        if (obj is AnEnum)      // Noncompliant
        {
            _ = (AnEnum)obj;    // Secondary
        }

        if (obj is AnEnum?)     // Noncompliant
        {
            _ = (AnEnum?)obj;   // Secondary
        }
    }

    void UserDefinedStructs(object obj)
    {
        if (obj is AStruct)             // Noncompliant
        {
            _ = (AStruct)obj;           // Secondary
        }

        if (obj is ARecordStruct)       // Noncompliant
        {
            _ = (ARecordStruct)obj;     // Secondary
        }

        if (obj is AReadonlyRefStruct)      // Noncompliant, but irrelevant, because ref structs cannot be casted
        {                                   // Error@+1 [CS0030] Cannot convert type 'object' to 'Repro_223.AReadonlyRefStruct'
            _ = (AReadonlyRefStruct)obj;    // Secondary
        }
    }

    enum AnEnum { Value1, Value2 }
    struct AStruct { }
    record struct ARecordStruct { }
    readonly ref struct AReadonlyRefStruct { }
}

public class FieldKeyword
{
    public string NonCompliant
    {
        get
        {
            if(field is string)       // Noncompliant
            {
                return (string)field; // Secondary
            }
            else
            {
                return (string)field;
            }
        }
        set;
    }
}
