using System;

public class ValidOrder
{
    public int publicOrInternal1;
    internal int publicOrInternal2;
    public int publicOrInternal3;
    internal int publicOrInternal4;

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
    private int private1;

    protected int protectedVariant1;            // Noncompliant {{Move this protected Field above the private ones.}}
    private protected int protectedVariant2;    // Noncompliant {{Move this protected Field above the private ones.}}
    protected internal int protectedVariant3;   // Noncompliant {{Move this protected Field above the private ones.}}

    public int publicOrInternal1;               // Noncompliant {{Move this public Field above the private ones.}}
    internal int publicOrInternal2;             // Noncompliant {{Move this internal Field above the private ones.}}
    public int publicOrInternal3;               // Noncompliant {{Move this public Field above the private ones.}}
    internal int publicOrInternal4;             // Noncompliant {{Move this internal Field above the private ones.}}
}

public class AboveProtected
{
    protected int protectedVariant;

    public int publicOrInternal1;               // Noncompliant {{Move this public Field above the protected ones.}}
    internal int publicOrInternal2;             // Noncompliant {{Move this internal Field above the protected ones.}}
}

public class AboveProtectedInternal
{
    protected internal int protectedVariant;

    public int publicOrInternal1;               // Noncompliant {{Move this public Field above the protected ones.}}
    internal int publicOrInternal2;             // Noncompliant {{Move this internal Field above the protected ones.}}
}

public class AboveProtectedPrivate
{
    protected private int protectedVariant;

    public int publicOrInternal1;               // Noncompliant {{Move this public Field above the protected ones.}}
    internal int publicOrInternal2;             // Noncompliant {{Move this internal Field above the protected ones.}}
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
    private const string Constant1 = "C1";
    public const string Constant2 = "C2";   // Noncompliant {{Move this public Constant above the private ones.}}

    private enum Enum1
    {
        None,
        All
    }

    public enum Enum2                       // Noncompliant {{Move this public Enum above the private ones.}}
    {
        None,
        Any,
        All
    }

    private readonly object field1 = new();
    private static readonly object field2 = new();
    public readonly object field3 = new();          // Noncompliant {{Move this public Field above the private ones.}}
    public static readonly object field4, field5;   // Noncompliant {{Move this public Field above the private ones.}}

    protected abstract int AbstractMethod1();
    protected abstract int AbstractProperty1 { get; }
    public abstract int AbstractMethod2();          // Noncompliant {{Move this public Abstract Member above the protected ones.}}
    public abstract int AbstractProperty2 { get; }  // Noncompliant {{Move this public Abstract Member above the protected ones.}}

    private delegate void SomeDelegate1();
    public delegate void SomeDelegate2();           // Noncompliant {{Move this public Delegate above the private ones.}}

    private event EventHandler SomeEvent1;
    public event EventHandler SomeEvent2;           // Noncompliant {{Move this public Event above the private ones.}}

    private object Property1 { get; } = 42;
    public object Property2 => 42;                  // Noncompliant {{Move this public Property above the private ones.}}

    private object this[int index] => 42;
    public object this[string name]                 // Noncompliant {{Move this public Indexer above the private ones.}}
    {
        get => 42;
    }

    private AllWrong() { }
    public AllWrong(int arg) { }                    // Noncompliant {{Move this public Constructor above the private ones.}}

    private void Method1() { }
    public void Method2() { }                       // Noncompliant {{Move this public Method above the private ones.}}
}

public record R
{
    private int shouldBeLast;
    public int shouldBeFirst;   // Noncompliant {{Move this public Field above the private ones.}}
}

public record struct RS
{
    private int shouldBeLast;
    public int shouldBeFirst;   // Noncompliant {{Move this public Field above the private ones.}}
}

public record struct S
{
    private int shouldBeLast;
    public int shouldBeFirst;   // Noncompliant {{Move this public Field above the private ones.}}
}
