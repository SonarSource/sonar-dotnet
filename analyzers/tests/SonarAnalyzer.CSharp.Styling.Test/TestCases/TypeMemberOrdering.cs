
using System;

public class CompliantClassSmall
{
    private const string Constant = "C";

    private static readonly object field = new();

    public object Property { get; }

    public void Method() { }
}

public abstract class CompliantClassFull
{
    private const string Constant1 = "C1";
    private const string Constant2 = "C2";

    public enum Enum1
    {
        None,
        All
    }

    private enum Enum2
    {
        None,
        Any,
        All
    }

    private readonly object field1 = new();
    private static readonly object field2 = new();
    private readonly object field3 = new();
    private readonly object field4, field5;

    public abstract int AbstractMethod1();
    public abstract int AbstractProperty1 { get; }
    public abstract int AbstractMethod2();
    public abstract int AbstractProperty2 { get; }

    delegate void SomeDelegate1();
    delegate void SomeDelegate2();

    public event EventHandler SomeEvent1;
    public event EventHandler SomeEvent2;

    public object Property1 { get; } = 42;
    public object Property2 => 42;
    public object Property3 { get => 42; }

    public object this[int index] => 42;
    public object this[string name]
    {
        get => 42;
    }

    public CompliantClassFull() { }
    private CompliantClassFull(int arg) { }

    ~CompliantClassFull() { }

    private void Method1() { }      // Compliant, this rule doesn't care about accessibility ordering
    public void Method2() { }

    public static int operator +(CompliantClassFull a, CompliantClassFull b) => 42;
    public static int operator -(CompliantClassFull a, CompliantClassFull b) => 42;
    public static implicit operator int(CompliantClassFull a) => 42;
    public static explicit operator CompliantClassFull(int a) => null;

    public class Nested1 { }        // Relative order of these types is not important
    private struct Nested2 { }
    public record Nested3 { }
    protected record struct Nested4 { }
    public record Nested5 { }
    public struct Nested6 { }
    public class Nested7 { }
}

public class WhenMemberShouldBeFirst
{
    public WhenMemberShouldBeFirst() { }

    public void Method() { }

    private readonly object field;  // Noncompliant {{Move Fields before Constructors.}}

    public class Nested { }
}

public class WhenMemberShouldBeLast
{
    private readonly object field;

    public class Nested { }

    public WhenMemberShouldBeLast() { }     // Noncompliant {{Move Constructors after Fields, before Methods.}}

    public void Method() { }                // Noncompliant {{Move Methods after Constructors, before Nested Types.}}
}

public class WhenMembersAreSwapped
{
    private readonly object field;

    public void Method() { }

    public WhenMembersAreSwapped() { }  // Noncompliant {{Move Constructors after Fields, before Methods.}}

    public class Nested { }
}

public class WhenLessMembersAreSwapped
{
    private const string constant = "C";

    public void Method1() { }
    public void Method2() { }

    // Only this one is out of place
    public WhenLessMembersAreSwapped() { } // Noncompliant {{Move Constructors after Constants, before Methods.}}
    //     ^^^^^^^^^^^^^^^^^^^^^^^^^

    public class Nested { }
}

public abstract class AllWrong
{
    public class Nested { }

    public void Method() { }    // Noncompliant {{Move Methods after Destructor, before Operators.}}
    //          ^^^^^^

    public static int operator +(AllWrong a, AllWrong b) => 42; // Noncompliant {{Move Operators after Methods, before Nested Types.}}
    //                         ^
    public static int operator -(AllWrong a, AllWrong b) => 42; // Noncompliant {{Move Operators after Methods, before Nested Types.}}
    public static implicit operator int(AllWrong a) => 42;      // Noncompliant {{Move Operators after Methods, before Nested Types.}}
    public static explicit operator AllWrong(int a) => null;    // Noncompliant {{Move Operators after Methods, before Nested Types.}}
    //                              ^^^^^^^^

    ~AllWrong() { }                   // Noncompliant {{Move Destructor after Constructors, before Methods.}}
//   ^^^^^^^^

    public AllWrong() { }                       // Noncompliant {{Move Constructors after Indexers, before Destructor.}}
    //     ^^^^^^^^

    public abstract int Abstract();             // Noncompliant {{Move Abstract Members after Fields, before Properties.}}
    //                  ^^^^^^^^

    public object this[int index] => 42;        // Noncomplinat {{Move Indexers after Properties, before Constructors.}}
    //            ^^^^

    public object Property { get; }             // Noncompliant {{Move Properties after Abstract Members, before Indexers.}}
    //            ^^^^^^^^

    private readonly object field1 = new();     // Noncompliant {{Move Fields after Enums, before Abstract Members.}}
    //               ^^^^^^^^^^^^^^^^^^^^^

    private readonly object field2, field3;     // Noncompliant {{Move Fields after Enums, before Abstract Members.}}
    //               ^^^^^^^^^^^^^^^^^^^^^

    public enum Enum { None, All }              // Noncompliant {{Move Enums after Constants, before Fields.}}
    //          ^^^^

    private const string Constant = "C", Answer = "42";   // Noncompliant {{Move Constants before Enums.}}
    //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
}

public record R (string Name)
{
    public void DoNothing() { }
    public int field;       // Noncompliant {{Move Fields before Methods.}}
}

public struct S
{
    public void DoNothing() { }
    public int field;       // Noncompliant {{Move Fields before Methods.}}
}

public record struct RS
{
    public void DoNothing() { }
    public int field;       // Noncompliant {{Move Fields before Methods.}}
}

public interface ISomething
{
    public void DoNothing();
    public int Value { get; }   // Noncompliant {{Move Properties before Methods.}}
}
