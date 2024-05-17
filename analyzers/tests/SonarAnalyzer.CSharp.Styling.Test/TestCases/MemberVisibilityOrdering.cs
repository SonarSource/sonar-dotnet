using System;

public class ValidOrder
{
    public int public1;
    public int public2;

    internal int internal1;
    internal int internal2;

    protected int protectedVariant1;
    private protected int protectedVariant2;
    protected internal int protectedVariant3;
    protected int protectedVariant4;
    private protected int protectedVariant5;
    protected internal int protectedVariant6;

    private int private1;
    private int private2;
}

public class AbovePrivate
{
    private int private1;                       // Secondary    [Private1, Private2, Private3, Private4, Private5, Private6, Private7]
    //      ^^^^^^^^^^^^

    protected int protectedVariant1;            // Noncompliant [Private1] {{Move this protected Field above the private ones.}}
    //        ^^^^^^^^^^^^^^^^^^^^^
    private protected int protectedVariant2;    // Noncompliant [Private2] {{Move this protected Field above the private ones.}}
    protected internal int protectedVariant3;   // Noncompliant [Private3] {{Move this protected Field above the private ones.}}

    public int public1;                         // Noncompliant [Private4] {{Move this public Field above the private ones.}}
    internal int internal1;                     // Noncompliant [Private5] {{Move this internal Field above the private ones.}}
    public int public2;                         // Noncompliant [Private6] {{Move this public Field above the private ones.}}
    internal int internal2;                     // Noncompliant [Private7] {{Move this internal Field above the private ones.}}
}

public class AboveProtected
{
    protected int protectedVariant;             // Secondary    [Protected1, Protected2]

    public int public1;                         // Noncompliant [Protected1] {{Move this public Field above the protected ones.}}
    internal int internal1;                     // Noncompliant [Protected2] {{Move this internal Field above the protected ones.}}
}

public class AboveProtectedInternal
{
    protected internal int protectedVariant;    // Secondary    [ProtectedInternal1, ProtectedInternal2]

    public int public1;                         // Noncompliant [ProtectedInternal1] {{Move this public Field above the protected ones.}}
    internal int internal1;                     // Noncompliant [ProtectedInternal2] {{Move this internal Field above the protected ones.}}
}

public class AboveProtectedPrivate
{
    protected private int protectedVariant;     // Secondary    [ProtectedPrivate1, ProtectedPrivate2]

    public int public1;                         // Noncompliant [ProtectedPrivate1] {{Move this public Field above the protected ones.}}
    internal int internal1;                     // Noncompliant [ProtectedPrivate2] {{Move this internal Field above the protected ones.}}
}

public abstract class CompliantClassFull
{
    public const string Constant1 = "C1";
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

    public readonly object field1 = new();
    public static readonly object field2 = new();
    private readonly object field3 = new();
    private static readonly object field4, field5;

    public abstract int AbstractMethod1();
    public abstract int AbstractProperty1 { get; }
    protected abstract int AbstractMethod2();
    protected abstract int AbstractProperty2 { get; }

    public delegate void SomeDelegate1();
    private delegate void SomeDelegate2();

    public event EventHandler SomeEvent1;
    private event EventHandler SomeEvent2;

    public object Property1 { get; } = 42;
    private object Property2 => 42;

    public object this[int index] => 42;
    private object this[string name]
    {
        get => 42;
    }

    public CompliantClassFull() { }
    private CompliantClassFull(int arg) { }

    ~CompliantClassFull() { }   // Not interesting

    public void Method1() { }
    private void Method2() { }

    public class Nested1 { }        // Relative order of these types is not important
    private struct Nested2 { }
    public record Nested3 { }
    protected record struct Nested4 { }
    public record Nested5 { }
    public struct Nested6 { }
    public class Nested7 { }
}


public abstract class AllWrong
{
    private const string Constant1 = "C1";  // Secondary    [Const]
    public const string Constant2 = "C2";   // Noncompliant [Const] {{Move this public Constant above the private ones.}}

    private enum Enum1                      // Secondary    [Enum]
    {
        None,
        All
    }

    public enum Enum2                       // Noncompliant [Enum] {{Move this public Enum above the private ones.}}
    {
        None,
        Any,
        All
    }

    private readonly object field1 = new();         // Secondary    [Field1, Field2]
    private static readonly object field2 = new();
    public readonly object field3 = new();          // Noncompliant [Field1] {{Move this public Field above the private ones.}}
    public static readonly object field4, field5;   // Noncompliant [Field2] {{Move this public Field above the private ones.}}

    protected abstract int AbstractMethod1();       // Secondary    [Abstract1, Abstract2]
    protected abstract int AbstractProperty1 { get; }
    public abstract int AbstractMethod2();          // Noncompliant [Abstract1] {{Move this public Abstract Member above the protected ones.}}
    public abstract int AbstractProperty2 { get; }  // Noncompliant [Abstract2] {{Move this public Abstract Member above the protected ones.}}

    private delegate void SomeDelegate1();          // Secondary    [Delegate]
    public delegate void SomeDelegate2();           // Noncompliant [Delegate] {{Move this public Delegate above the private ones.}}

    private event EventHandler SomeEvent1;          // Secondary    [Event]
    public event EventHandler SomeEvent2;           // Noncompliant [Event] {{Move this public Event above the private ones.}}

    private object Property1 { get; } = 42;         // Secondary    [Property]
    public object Property2 => 42;                  // Noncompliant [Property] {{Move this public Property above the private ones.}}

    private object this[int index] => 42;           // Secondary    [Indexer]
    public object this[string name]                 // Noncompliant [Indexer] {{Move this public Indexer above the private ones.}}
    {
        get => 42;
    }

    private AllWrong() { }                          // Secondary    [Constructor]
    public AllWrong(int arg) { }                    // Noncompliant [Constructor] {{Move this public Constructor above the private ones.}}

    private void Method1() { }                      // Secondary    [Method]
    public void Method2() { }                       // Noncompliant [Method] {{Move this public Method above the private ones.}}
}

public record R
{
    private int shouldBeLast;   // Secondary    [InRecord]
    public int shouldBeFirst;   // Noncompliant [InRecord] {{Move this public Field above the private ones.}}
}

public record struct RS
{
    private int shouldBeLast;   // Secondary    [InRecordStruct]
    public int shouldBeFirst;   // Noncompliant [InRecordStruct] {{Move this public Field above the private ones.}}
}

public record struct S
{
    private int shouldBeLast;   // Secondary    [InStruct]
    public int shouldBeFirst;   // Noncompliant [InStruct] {{Move this public Field above the private ones.}}
}
