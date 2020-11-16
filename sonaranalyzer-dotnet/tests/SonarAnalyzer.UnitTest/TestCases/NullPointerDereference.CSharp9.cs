using System;
using System.Text;

object topLevel = null;
topLevel.ToString();   // FN

topLevel = "Value";
topLevel.ToString();

void TopLevelLocalFunction()
{
    object local = null;
    local.ToString();

    local = "Value";
    local.ToString();
}

public class Sample
{
    private string field;

    public void TargetTypedNew()
    {
        StringBuilder sb = null;
        sb.ToString(); // FN, can't build CFG for this method

        sb = new(42);
        sb.Append("Value");
    }

    public void PatternMatching(object arg)
    {

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
            arg.ToString();     // FN
        }
        else
        {
            object o = null;
            if (o is not null)
            {
                o.ToString();
            }
            o.ToString();       // FN, can't build CFG for this method
        }
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
            field = o.ToString();
        }
    }

    public object PropertyWithValue
    {
        get => null;
        init
        {
            if (value == null)
            {
                field = value.ToString();   // FN
            }
        }
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

