using System;
using System.Text;

object topLevel = "Value";
topLevel.ToString();

topLevel = null;
topLevel.ToString();   // Noncompliant

void TopLevelLocalFunction()
{
    object local = "Value";
    local.ToString();

    local = null;
    local.ToString();   // Noncompliant
}

public class Sample
{
    private string field;

    public void TargetTypedNew()
    {
        StringBuilder sb;

        sb = new();
        sb.Append("Value");

        sb = new(42);
        sb.Append("Value");

        sb = null;
        sb.ToString(); // Noncompliant
    }

    public void PatternMatching(object arg)
    {
        object o = null;
        if (arg is string)
        {
            arg.ToString();     // Compliant
        }

        if (arg is int and > 0 and > 1)
        {
            arg.ToString();     // Compliant
        }

        if (arg is int or bool or long)
        {
            arg.ToString();     // Compliant
        }

        if (arg is null)
        {
            arg.ToString();     // Noncompliant
        }
        if (arg is int or bool or null)
        {
            arg.ToString();     // FN
        }
        else if (arg is not not null)
        {
            arg.ToString();     // Compliant the if branch covers null so arg is NotNull
        }
        else if (!(arg is not null))
        {
            arg.ToString();     // Compliant the if branch covers null so arg is NotNull
        }
        else
        {
            if (o is not null)
            {
                o.ToString();   // Compliant
            }
            o.ToString();       // Noncompliant
        }

        if (arg is false)
        {
            if ((bool)arg)
            {
                o.ToString();   // Compliant, unreachable
            }
            else
            {
                o.ToString();   // Noncompliant
            }
        }

        if (arg is true)
        {
            if (!(bool)arg)
            {
                o.ToString();   // Compliant, unreachable
            }
            else
            {
                o.ToString();   // Noncompliant
            }
        }
    }

    public void PatternMatching_NotNotNull(object arg)
    {
        if (arg is not not null)
        {
            arg.ToString();     // Noncompliant
        }
        else
        {
            arg.ToString();     // Compliant
        }
    }

    public void PatternMatching_NotNotNull_Mixed(object arg)
    {
        if (!(arg is not null))
        {
            arg.ToString();     // Noncompliant
        }
        else
        {
            arg.ToString();     // Compliant
        }
    }

    public void PatternMatching_Recursive(object arg)
    {
        if (arg is { })
        {
            arg.ToString();     // Compliant
        }
        else
        {
            arg.ToString();     // Noncompliant
        }
    }

    public void PatternMatching_Recursive_Negated(object arg)
    {
        if (arg is not { })
        {
            arg.ToString();     // Noncompliant
        }
        else
        {
            arg.ToString();     // Compliant
        }
    }

    public void PatternMatchingSwitch(object arg)
    {
        var res = arg switch
        {
            not null => arg,
            _ => ""
        };
        res.ToString();

        res = arg switch
        {
            string and not null => arg,
            _ => ""
        };
        res.ToString();     // Compliant

        res = arg switch
        {
            string x => x,
            not not null => arg,
            _ => ""
        };
        res.ToString();     // Noncompliant

        object o = null;
        o.ToString();       // Noncompliant
    }

    public void PatternMatching_Numbers()
    {
        object o = null;
        var value = 42;
        o = value switch
        {
            < 0 => o.ToString(),            // Compliant, unreachable
            <= 0 => o.ToString(),           // Compliant, unreachable
            > 100 => o.ToString(),          // Compliant, unreachable
            >= 100 => o.ToString(),         // Compliant, unreachable
            > 0 and < 20 => o.ToString(),   // Compliant, unreachable
            < 10 or > 90 => o.ToString(),   // Compliant, unreachable
            > 40 and < 50 => o.ToString(),  // Noncompliant, this is reachable
            _ => o.ToString()               // Compliant, unreachable due to previous line
        };
    }

    public void LearnFromPatternMatchingEverywhere(object arg)
    {
        Something(arg is null);
        arg.ToString(); // Noncompliant

        void Something(bool b)
        { }
    }

    public void StaticLambda()
    {
        Func<string> a = static () =>
        {
            object o = null;
            return o.ToString();    // Noncompliant
        };
        a();
    }

    public int PropertySimple
    {
        get => 42;
        init
        {
            object o = null;
            field = o.ToString();   // Noncompliant
        }
    }

    public object PropertyWithValue
    {
        get => null;
        init
        {
            if (value == null)
            {
                field = value.ToString();   // Noncompliant
            }
        }
    }

    void Nullable<T>() where T : struct
    {
        T? localTargetTypeNew = new();
        localTargetTypeNew.GetType();   // Compliant localTargetTypeNew is 0
    }
}

public record Record
{
    public void Method()
    {
        object o = null;
        o.ToString();   // Noncompliant
    }
}

public partial class Partial
{
    public partial void Method();
}

public partial class Partial
{
    public partial void Method()
    {
        object o = null;
        o.ToString();   // Noncompliant
    }
}

namespace TartetTypedConditional
{
    public interface IAlpha { }
    public interface IBeta { }
    public class AlphaAndBeta : IAlpha, IBeta { }
    public class BetaAndAlpha : IAlpha, IBeta { }

    public class Sample
    {
        public void Go(bool condition)
        {
            AlphaAndBeta ab = new AlphaAndBeta();
            BetaAndAlpha ba = null;
            IAlpha result = condition ? ab : ba;
            result.ToString();  // Noncompliant
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7665
namespace Repro7665
{
    class IsNotAndShortCircuitOr
    {
        void FlatVersion(ObjectInstance obj)
        {
            if (obj?.Value is not Table)
            {
                obj.ToString(); // Noncompliant
            }
            else
            {
                obj.ToString(); // Compliant, "null is not Table" returns True. So this path is visited only for NotNull.
            }
        }

        void Test(ObjectInstance obj)
        {
            _ = obj?.Value is not Table || obj.Value is Table; // Compliant, "null is not Table" returns True. So || is not visited.
            obj = Unknown();
            _ = obj?.Value is Table || obj.Value is Table;     // Noncompliant
            obj = Unknown();
            _ = obj?.Value is Table && obj.Value is Table;     // Compliant
        }

        private ObjectInstance Unknown() => null;
    }

    record Table;
    class ObjectInstance
    {
        public object Value { get; }
    }
}
