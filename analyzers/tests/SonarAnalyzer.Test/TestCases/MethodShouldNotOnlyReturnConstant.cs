public class Program
{
    public int GetInt() // Noncompliant {{Remove this method and declare a constant for this value.}}
//             ^^^^^^
    {
        return 12;
    }

    public int GetMultiplication()
    {
        return 3 * 2;
    }

    public int GetAge(string name) // Compliant - method takes parameters
    {
        return 12;
    }

    private double GetDouble() // Noncompliant
    {
        return (((3.14)));
    }

    private int GetArrow() => 42; // Noncompliant

    public string GetString() // Noncompliant
    {
        return "foo";
    }

    public bool GetIsEnabled()  // Noncompliant
    {
        return true;
    }

    public string GetNull()
    {
        return null;
    }

    public char GetChar() // Noncompliant
    {
        return 'a';
    }

    string GetWithNoModifier() // Noncompliant
    {
        return "";
    }

    public int GetWithInner()
    {
        return GetInner();

        int GetInner() // Compliant - FN - should not be compliant
        {
            return 42;
        }
    }

    private const int CONSTANT = 42;

    public int ReturnsConstant()
    {
        return CONSTANT;  // Compliant, not a literal expression
    }
}

public interface IFoo
{
    int GetValue();
}

public class Foo : IFoo
{
    public int GetValue() // Compliant - implements interface so cannot get rid of the method
    {
        return 42;
    }
}

public abstract class Base
{
    protected virtual string GetName() // Compliant - can be overriden
    {
        return "";
    }

    public abstract float GetPrecision();
}

public class NotBase : Base
{
    protected override string GetName() // Compliant - override
    {
        return "John";
    }

    public override float GetPrecision() // Compliant - override
    {
        return 0.1F;
    }
}

public static class Extensions
{
    public static string ClassicCompliant(this string s) => s + "!";
    public static string ClassicNonCompliant(this string s) => "!";  // FN https://sonarsource.atlassian.net/browse/NET-2733
}

// https://sonarsource.atlassian.net/browse/NET-3640
public class Repro_3640
{
    public int InConditionalCompilation()
    {
#if NET
        return 42;  // Compliant
#else
        return 24;  // Compliant
#endif
    }

    public int OutsideConditionalCompilation()
    {
#if SOMETHING
        return 24;
#endif
        return 42;  // Compliant, ugly due to missing #else, but compliant
    }

    public int Arrow() =>
#if NET
        42;  // Compliant
#else
        24;  // Compliant
#endif

    public int TruePositive() =>    // Noncompliant
        42;

#if SOMETHING
    public void ThisIsUnrelatedLeadingTrivia_BeforeArrow() { }
#endif

    public int ConditionalLeadingTrivia_Arrow() =>  // Noncompliant
        42;

#if SOMETHING
    public void ThisIsUnrelatedLeadingTrivia_BeforeBody() { }
#endif

    public int ConditionalLeadingTrivia_Body()      // Noncompliant
    {
        return 42;
    }

#if SOMETHING
    public void ThisIsUnrelatedTrailingTrivia_AfterBody() { }
#endif

}

public class InvalidCode
{
    public int MethodWithoutBody(); // Error [CS0501] 'InvalidCode.MethodWithoutBody()' must declare a body because it is not marked abstract, extern, or partial
}
