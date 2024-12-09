using System;

namespace NS
{
    public class PrecededByParenthesis() { }    // Compliant
    public class Adjacent { }                   // Noncompliant {{Add an empty line before this declaration.}}
//  ^^^^^^
}
namespace AdjacentNS { }                        // Noncompliant

public abstract class CompliantClassFull
{
    private const string Constant1 = "C1";
    private const string Constant2 = "C2";

    public enum Enum1
    {
        None,
        All
    }

    public enum Enum2
    {
        None,
        All
    }

    private enum Enum3 { None, Any, All }

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
    public object Property3 => 43;
    public object Property4 { get => 42; }
    public object Property5 { get; set; }
    public object Property6 { get; set; }

    public object this[int index] => 42;
    public object this[bool value] => 42;

    public object this[string name]
    {
        get => 42;
    }

    public CompliantClassFull() { }

    private CompliantClassFull(int arg) { }

    ~CompliantClassFull() { }

    private void Method1() { }

    public void Method2() { }

    public static int operator +(CompliantClassFull a, CompliantClassFull b) => 42;

    public static int operator -(CompliantClassFull a, CompliantClassFull b) => 42;

    public static implicit operator int(CompliantClassFull a) => 42;

    public static explicit operator CompliantClassFull(int a) => null;

    public class Nested1 { }

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
    private const string Constant2 = "C2";
    public enum Enum1   // Noncompliant
    {
        None,
        All
    }
    private enum Enum2  // Noncompliant
    {
        None,
        Any,
        All
    }
    private enum Enum3 {  None } // Noncompliant
    private readonly object field1 = new();         // Noncompliant
    private static readonly object field2 = new();
    private readonly object field3 = new();
    private readonly object field4, field5;
    public abstract int AbstractMethod1();          // FN
    public abstract int AbstractProperty1 { get; }
    delegate void SomeDelegate1();                  // Noncompliant
    delegate void SomeDelegate2();
    public event EventHandler SomeEvent1;           // Noncompliant
    public event EventHandler SomeEvent2;
    public object Property1 { get; } = 42;          // Noncompliant
    public object Property2 => 42;
    public object Property3 { get => 42; }
    public object this[int index] => 42;            // Noncompliant
    public object this[bool value] => 42;
    public object this[string name]                 // Noncompliant
    {
        get => 42;
    }
    public AllWrong() { }           // Noncompliant
    private AllWrong(int arg) { }   // Noncompliant
    ~AllWrong() { }                 // Noncompliant
    private void Method1() { }      // Noncompliant
    public void Method2() { }       // Noncompliant
    public static int operator +(AllWrong a, AllWrong b) => 42; // Noncompliant
    public static int operator -(AllWrong a, AllWrong b) => 42; // Noncompliant
    public static implicit operator int(AllWrong a) => 42;                // Noncompliant
    public static explicit operator AllWrong(int a) => null;              // Noncompliant
    public class Nested1 { }                // Noncompliant
    private struct Nested2 { }              // Noncompliant
    public record Nested3 { }               // Noncompliant
    protected record struct Nested4 { }     // Noncompliant
}

public class Comments
{
    public class Compliant { }

    // This is fine
    public class CompliantSingleSingleLine { }
    // Missplaced comment

    // This is not fine, but it is compliant
    public class CompliantTwoSeparatedSingleLine { }

    /*
     * Fine for multiline
     */
    public class CompliantSingleMultiLine1 { }

    /* And multiline on a single line */
    public class CompliantSingleMultiLine2 { }

    /* And multiline on a single line */
    /* Alaso if there are multiple */
    public class CompliantMultipleMultiLine { }

    /// <summary>
    /// Documentation is fine
    /// </summary>
    public class CompliantDocumentation { }

    /// <summary>
    /// This is broken, but still compliant

    /// </summary>
    public class CompliantDocumentationInterrupted { }

    /**
      * <summary>
      * There still should be an empty line before the documentation block
      * </summary>
      */
    public class CompliantMultilineDocumentation { }

    public class ProblemsStartBelowThisLine { }
    // There still should be empty line before the comment
    public class SingleSingleLine { }       // Noncompliant@-1
    // There still should be empty line before the comment
    // Also if there are multiple
    public class MultipleSingleLine { }     // Noncompliant@-2
    /*
     * Fine for multiline
     */
    public class SingleMultiLine1 { }       // Noncompliant@-3
    /* And multiline on a single line */
    public class SingleMultiLine2 { }       // Noncompliant@-1
    /* And multiline on a single line */
    /* Alaso if there are multiple */
    public class MultipleMultiLine { }      // Noncompliant@-2
    /// <summary>
    /// There still should be an empty line before the documentation block
    /// </summary>
    public class Documentation { }          // Noncompliant@-3
    /// <summary>
    /// There still should be an empty line before the documentation block, not inside

    /// </summary>
    public class InterruptedDocumentation { }   // Noncompliant@-4
    /**
      * <summary>
      * There still should be an empty line before the documentation block
      * </summary>
      */
    public class MultilineDocumentation { } // Noncompliant@-5
}

public class MultiLines
{
    private int singleLineField1;
    private int multiLineField1 =   // Noncompliant
        1 + 1;
    private int singleLineField2;

    private int multiLineField2 =
        1 + 1;

    private int singleLineField3;

    public int SingleLineProperty1 => 42;
    public int MultiLineProperty1               // Noncompliant
    {
        get => 42;
    }
    public int SingleLineProperty2 => 42;       // Noncompliant

    public int MultiLineProperty2
    {
        get => 42;
    }

    public int SingleLineProperty3 => 42;

    public event EventHandler SingleLineEvent1;
    public event EventHandler MultiLineEvent1       // Noncompliant
    {
        add { }
        remove { }
    }
    public event EventHandler SingleLineEvent2;     // Noncompliant

    public event EventHandler MultiLineEvent2
    {
        add { }
        remove { }
    }

    public event EventHandler SingleLineEvent3;

    public object this[int index] => 42;
    public object this[string name]                 // Noncompliant
    {
        get => 42;
    }
    public object this[bool condition] => 42;       // Noncompliant

    public object this[double value]
    {
        get => 42;
    }

    public object this[decimal value] => 42;
}

public interface ISomething
{
    void SayHello();    // All compliant
    int Property { get; }
    int DoSomething();
    int MultiLineProperty
    {
        get;
        set;
    }
    void DoNothing();
    void DoNothingAtAll();
}
