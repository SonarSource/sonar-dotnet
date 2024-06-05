
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
    public WhenMemberShouldBeFirst() { }    // Secondary [First] {{Move the declaration before this one.}}

    public void Method() { }

    private readonly object field;          // Noncompliant [First] {{Move Fields before Constructors.}}

    public class Nested { }
}

public class WhenMemberShouldBeLast
{
    private readonly object field;

    public class Nested { }                 // Secondary [Last2] {{Move the declaration before this one.}}

    public WhenMemberShouldBeLast() { }     // Noncompliant [Last1] {{Move Constructors after Fields, before Methods.}}

                                            // Secondary@+1 [Last1]
    public void Method() { }                // Noncompliant [Last2] {{Move Methods after Constructors, before Nested Types.}}
}

public class WhenMembersAreSwapped
{
    private readonly object field;

    public void Method() { }            // Secondary [Swapped1] {{Move the declaration before this one.}}

    public WhenMembersAreSwapped() { }  // Noncompliant [Swapped1] {{Move Constructors after Fields, before Methods.}}

    public class Nested { }
}

public class WhenLessMembersAreSwapped
{
    private const string constant = "C";

    public void Method1() { }               // Secondary [Swapped2] {{Move the declaration before this one.}}
    public void Method2() { }

    // Only this one is out of place
    public WhenLessMembersAreSwapped() { }  // Noncompliant [Swapped2] {{Move Constructors after Constants, before Methods.}}
    //     ^^^^^^^^^^^^^^^^^^^^^^^^^

    public class Nested { }
}

public class InterleavedViolations
{
    private int field1;
    public int Property1 { get; set; }  // Secondary    [Interleaved1, Interleaved3]
    private int field2;                 // Noncompliant [Interleaved1] {{Move Fields before Properties.}}
    void Method1() { }                  // Secondary    [Interleaved2, Interleaved4]
    public int Property2 { get; set; }  // Noncompliant [Interleaved2] {{Move Properties after Fields, before Methods.}}
    void Method2() { }
    private int field3;                 // Noncompliant [Interleaved3] {{Move Fields before Properties.}}
    void Method3() { }
    public int Property3 { get; set; }  // Noncompliant [Interleaved4] {{Move Properties after Fields, before Methods.}}
}

public abstract class AllWrong
{
    public class Nested { }     // Secondary [AllWrongOperator1, AllWrongOperator2, AllWrongOperator3, AllWrongOperator4]

                                // Secondary@+1 [AllWrongDestructor]
    public void Method() { }    // Noncompliant [AllWrongMethod1] {{Move Methods after Destructor, before Operators.}}
    //          ^^^^^^

    public void Method2() { }   // Noncompliant [AllWrongMethod2] {{Move Methods after Destructor, before Operators.}}
                                                                // Secondary@+1 [AllWrongMethod1, AllWrongMethod2] This secondary is not very useful, because it's already where it should be. "After" part would be more helpful.
    public static int operator +(AllWrong a, AllWrong b) => 42; // Noncompliant [AllWrongOperator1] {{Move Operators after Methods, before Nested Types.}}
    //                         ^
    public static int operator -(AllWrong a, AllWrong b) => 42; // Noncompliant [AllWrongOperator2] {{Move Operators after Methods, before Nested Types.}}
    public static implicit operator int(AllWrong a) => 42;      // Noncompliant [AllWrongOperator3] {{Move Operators after Methods, before Nested Types.}}
    public static explicit operator AllWrong(int a) => null;    // Noncompliant [AllWrongOperator4] {{Move Operators after Methods, before Nested Types.}}
    //                              ^^^^^^^^

                                                // Secondary@+1 [AllWrongConstructor]
    ~AllWrong() { }                             // Noncompliant [AllWrongDestructor]    {{Move Destructor after Constructors, before Methods.}}
//   ^^^^^^^^

    // Secondary@+1 [AllWrongIndexer]
    public AllWrong() { }                       // Noncompliant [AllWrongConstructor]   {{Move Constructors after Indexers, before Destructor.}}
    //     ^^^^^^^^

                                                // Secondary@+1 [AllWrongField1, AllWrongField2]
    public abstract int Abstract();             // Noncompliant [AllWrongAbstract]      {{Move Abstract Members after Fields, before Properties.}}
    //                  ^^^^^^^^
                                                // Secondary@+1 [AllWrongProperty]
    public object this[int index] => 42;        // Noncompliant [AllWrongIndexer]       {{Move Indexers after Properties, before Constructors.}}
    //            ^^^^

                                                // Secondary@+1 [AllWrongAbstract]
    public object Property { get; }             // Noncompliant [AllWrongProperty]      {{Move Properties after Abstract Members, before Indexers.}}
    //            ^^^^^^^^

                                                // Secondary@+1 [AllWrongEnum]
    private readonly object field1 = new();     // Noncompliant [AllWrongField1]        {{Move Fields after Enums, before Abstract Members.}}
    //               ^^^^^^^^^^^^^^^^^^^^^

    private readonly object field2, field3;     // Noncompliant [AllWrongField2]        {{Move Fields after Enums, before Abstract Members.}}
    //               ^^^^^^^^^^^^^^^^^^^^^

                                                // Secondary@+1 [AllWrongConst]
    public enum Enum { None, All }              // Noncompliant [AllWrongEnum]          {{Move Enums after Constants, before Fields.}}
    //          ^^^^

    private const string Constant = "C", Answer = "42";   // Noncompliant [AllWrongConst] {{Move Constants before Enums.}}
    //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
}

public record R (string Name)
{
    public void DoNothing() { } // Secondary [R]
    public int field;           // Noncompliant [R] {{Move Fields before Methods.}}
}

public struct S
{
    public void DoNothing() { } // Secondary [S]
    public int field;           // Noncompliant [S] {{Move Fields before Methods.}}
}

public record struct RS
{
    public void DoNothing() { } // Secondary [RS]
    public int field;           // Noncompliant [RS] {{Move Fields before Methods.}}
}

public interface ISomething
{
    public void DoNothing();    // Secondary [ISomething]
    public int Value { get; }   // Noncompliant [ISomething] {{Move Properties before Methods.}}
}
