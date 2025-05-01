using System;
using System.Linq.Expressions;

public class Sample
{
    public void Method(object o, int a, int b, object another)
    {
        if (o == null)  // Noncompliant {{Use 'is null' pattern matching.}}
        //  ^^^^^^^^^
        { }
        if (a > b || (a < b && o == null))  // Noncompliant
        //                     ^^^^^^^^^
        { }

        if (o != null)  // Noncompliant {{Use 'is not null' pattern matching.}}
        //  ^^^^^^^^^
        { }

        _ = !(o == null);   // Noncompliant
        //    ^^^^^^^^^
        _ = !(o != null);   // Noncompliant
        //    ^^^^^^^^^
        _ = null == o;      // Noncompliant
        _ = null != o;      // Noncompliant

        _ = o is null;
        _ = o is not null;
        _ = o == "null";
        _ = o != "null";
        _ = o == this;
        _ = o != this;
        _ = o == another;
        _ = o != another;
        _ = o == Invocation();
        _ = o != Invocation();

        Use(o == null);     // Noncompliant
        while (o == null)   // Noncompliant
        { }

        Use(o ==);  // Error [CS1525] Invalid expression term ';'
        Use(== o);  // Error [CS1525] Invalid expression term '=='
    }

    private object Invocation() => null;

    private void Use(bool b) { }

    public void CustomOperator()
    {
        var s = new Sample();
        if(s == null)   // Noncompliant
        {
            // This is always visited due to overriden operator => we don't care, it's a bad idea anyway
        }
    }

    public void ExpressionTree(object x)
    {
        Func<bool> func1 = () => x == null; // Noncompliant
        Func<bool> func2 = () => x is null;
        Expression<Func<bool>> expression1 = () => x == null;
        Expression<Func<bool>> expression2 = () => x != null;
        Expression<Func<bool>> expression3 = () => x is null; // Error [CS8122]
    }

    public static bool operator ==(Sample a, Sample b) =>
        b is null;

    public static bool operator !=(Sample a, Sample b) =>
        b is not null;
}

public class IsPattern
{
    public void Method(object value, Exception ex)
    {
        _ = value is { };           // Noncompliant {{Use 'is not null' pattern matching.}}
        //           ^^^
        _ = ex is { Message: { } }; // Noncompliant {{Use 'is not null' pattern matching.}}
        //                   ^^^
    }

    public void Compliant(object value, string str, Exception ex)
    {
        _ = value is null;
        _ = value is not null;
        _ = value is { } renamed;
        _ = Value() is { } captured;
        _ = value is Exception;
        _ = value is Exception e;
        _ = str is { Length: 0 };
        _ = ex is { Message: { } message };
    }

    private object Value() => null;
}
