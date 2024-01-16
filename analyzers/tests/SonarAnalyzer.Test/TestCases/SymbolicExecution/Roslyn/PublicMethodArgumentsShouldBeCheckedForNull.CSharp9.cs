using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class Sample
{
    private string field;

    public void TargetTypedNew(object arg)
    {
        arg.ToString();         // Noncompliant

        StringBuilder sb = new();
    }

    public void PatternMatching(object arg)
    {
        if (arg is string)
        {
            arg.ToString();     // Compliant
        }

        if (arg is int and > 0 and > 1)
        {
            arg.ToString();     // Noncompliant - FP
        }

        if (arg is int or bool or long)
        {
            arg.ToString();     // Noncompliant - FP
        }

        if (arg is null)
        {
            arg.ToString();     // Covered by S2259
        }
        if (arg is int or bool or null)
        {
            arg.ToString();
        }
        else if (arg is not not null)
        {
            arg.ToString();
        }
        else if (!(arg is not null))
        {
            arg.ToString();
        }
    }

    public void StaticLambdas()
    {
        MethodAcceptsFunction(static obj => obj.ToString());
        MethodAcceptsFunction(static (obj) => obj.ToString());
    }

    private void MethodAcceptsFunction(Action<object> action) { }

    public object PropertySet
    {
        get => null;
        set
        {
            field = value.ToString();   // Noncompliant
        }
    }

    public object PropertyInit
    {
        get => null;
        init
        {
            field = value.ToString();   // Noncompliant
        }
    }
}

public record Record
{
    public void Method(object arg)
    {
        arg.ToString();                 // Noncompliant
    }
}

public partial class Partial
{
    public partial void Method(object arg);
}

public partial class Partial
{
    public partial void Method(object arg)
    {
        arg.ToString();                 // Noncompliant
    }
}

public class UsingFromServicesAttribute
{
    public int GetProduct([FromServices] IService service) =>
         service.GetValue();            // Compliant, it's attributed with FromServices attribute

    public int GetProductMultipleAttr([FromServices][FromRoute] IService service) =>
        service.GetValue();             // Compliant, it's attributed with FromServices attribute

    public int GetPrice(IService service) =>
        service.GetValue();             // Noncompliant

    public interface IService
    {
        int GetValue();
    }
}

public class Assignments
{
    public void CoalesceAssignment_NotNull(object o)
    {
        o ??= new object();
        o.ToString(); // Compliant
    }

    public void CoalesceAssignment_Unknown(object o)
    {
        o ??= Unknown();
        o.ToString(); // Noncompliant - FP: parameter reassignment via a null coalesce assignment is not supported
    }

    public void CoalesceAssignment_Null(object o)
    {
        o ??= null;
        o.ToString(); // Compliant
    }

    public void NullCoalesce_NotNull(object o)
    {
        o = o ?? new object();
        o.ToString(); // Compliant
    }

    public void NullCoalesce_Unknown(object o)
    {
        o = o ?? Unknown();
        o.ToString(); // Noncompliant - FP
    }

    public void NullCoalesce_Null(object o)
    {
        o = o ?? null;
        o.ToString(); // Compliant
    }

    public void NullCoalesce_Throw(object o)
    {
        o = o ?? throw new ArgumentNullException("");
        o.ToString(); // Compliant
    }

    public void NullConditional(object o)
    {
        o = o?.ToString() ?? "";
        o.ToString(); // Compliant
    }

    public void TernaryAssignment(object o)
    {
        o = o == null ? Unknown() : o;
        o.ToString(); // Noncompliant - FP
    }

    public void TernaryThrow1(object o)
    {
        o = o == null ? throw new ArgumentNullException("") : Unknown();
        o.ToString(); // Noncompliant - FP
    }

    public void TernaryThrow2(object o)
    {
        o = o == null ? throw new ArgumentNullException("") : o;
        o.ToString(); // Compliant
    }

    public void StringConcatenation(string s)
    {
        s = s + "Test";
        s.ToString(); // Compliant
    }

    public void CompundAssignment_String(string s)
    {
        s += "Test";
        s.ToString(); // Compliant, += never returns null
    }

    public void CompundAssignment_NullabaleInt(int? i)
    {
        i *= 2;
        i.GetType(); // Compliant: don't raise for nullable value types
    }

    private object Unknown() => null;
}

public class ThrowIfNull
{
    public void ThrowIfNullString(string s)
    {
        ArgumentNullException.ThrowIfNull(s, nameof(s));
        s.ToString(); // Compliant
    }

    public void ThrowIfNullOrEmptyString(string s)
    {
        ArgumentException.ThrowIfNullOrEmpty(s, nameof(s));
        s.ToString(); // Compliant
    }
}
