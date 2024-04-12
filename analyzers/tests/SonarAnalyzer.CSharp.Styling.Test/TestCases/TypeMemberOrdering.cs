
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

    public object Property1 { get; } = 42;
    public object Property2 => 42;
    public object Property3 { get => 42; }

    public CompliantClassFull() { }
    private CompliantClassFull(int arg) { }

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

    public class Nested { }         // Noncompliant {{Move Nested Types after Methods.}}

    public WhenMemberShouldBeLast() { }

    public void Method() { }
}

public class WhenMembersAreSwapped
{
    private readonly object field;

    public void Method() { }            // Noncompliant {{Move Methods after Constructors, before Nested Types.}}

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

    public class Nested { }
}

public abstract class AllWrong
{
    public class Nested { }     // Noncompliant {{FIXME}}
    //           ^^^^^^

    public void Method() { }    // Noncompliant {{FIXME}}
    //          ^^^^^^

    public static int operator +(AllWrong a, AllWrong b) => 42; // Noncompliant {{FIXME}}
    //                ^^^^^^^^^^
    public static int operator -(AllWrong a, AllWrong b) => 42; // Noncompliant {{FIXME}}
    public static implicit operator int(AllWrong a) => 42;                // Noncompliant {{FIXME}}
    public static explicit operator AllWrong(int a) => null;              // Noncompliant {{FIXME}}

    public AllWrong() { }               // Noncompliant {{FIXME}}
    //     ^^^^^^^^

    public abstract int Abstract();     // Noncompliant {{FIXME}}
    //                  ^^^^^^^^

    public object Property { get; }     // Noncompliant {{FIXME}}
    //            ^^^^^^^^

    private readonly object field1 = new();      // Noncompliant {{FIXME}}
    //                      ^^^^^^

    private readonly object field2, field3;      // Noncompliant {{FIXME}}
    //                      ^^^^^^^^^^^^^^

    public enum Enum { None, All }              // Noncompliant {{FIXME}}
    //          ^^^^

    private const string Constant = "C";        // Noncompliant {{FIXME}}
    //                   ^^^^^^^^
}

// FIXME: Record, struct, recod struct, interface
// FIXME: Delegate
// FIXME: Incomplete member
// FIXME: Event a field stejne
// FIXME: Desctructor
// FIXME: Indexer

