using System.Collections.Generic;
using System.Text;

public class Program
{
    public StringBuilder MyMethod(StringBuilder builder, string myString) // Compliant
    {
        StringBuilder builder1 = GetStringBuilder(); // Compliant

        var builder2 = new StringBuilder(); // Compliant
        ExternalMethod(builder2);

        var builder3 = new StringBuilder(); // Compliant
        LocalMethod(builder3);

        StringBuilder builder4 = new StringBuilder(); // Compliant
        builder4.ToString();

        var builder5 = new StringBuilder(); // Compliant

        StringBuilder builder6 = new StringBuilder(); // Noncompliant {{Remove this "StringBuilder"; ".ToString()" is never called.}}
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        var builder7 = new StringBuilder(); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        StringBuilder builder8 = new StringBuilder(); // Noncompliant
        builder8.Append(builder5.ToString());

        StringBuilder builder9 = new StringBuilder(); // Compliant
        var a = builder9.ToString();

        StringBuilder builder10 = new StringBuilder(); // Compliant
        var b = builder10.ToString().ToLower();

        StringBuilder builder11 = new StringBuilder(); // Compliant
        var c = builder11.Append("").Append("").Append("").Append("").ToString().ToLower();

        StringBuilder builder12 = new StringBuilder(); // Compliant
        builder12.CopyTo(0, new char[1], 0, 1);

        StringBuilder builder13 = new StringBuilder(); // Compliant
        var d = builder13.GetChunks();

        StringBuilder builder14 = new StringBuilder(); // Noncompliant FP
        var m = builder14[0];

        StringBuilder builder15 = new StringBuilder(); // Noncompliant FP
        builder15?.Append("").ToString();

        if (true)
        {
            if (true)
            {
                if (true)
                {
                    if (true)
                    {
                        if (true)
                        {
                            var builder16 = new StringBuilder(); // Noncompliant
                        }
                    }
                }
            }
        }

        (StringBuilder, StringBuilder) builderTuple = (new StringBuilder(), new StringBuilder()); // FN

        StringBuilder builderCfg = new StringBuilder(); // FN (we should use cfg with significant impact on performance)
        if (false)
        {
            builderCfg.ToString();
        }

        StringBuilder builderCalls = new StringBuilder(); // Noncompliant
        builderCalls.Append("Append");
        builderCalls.AppendLine("AppendLine");
        builderCalls.Replace("\r\n", "\n");
        builderCalls.Clear();
        builderCalls.GetType();

        var builderCalls2 = new StringBuilder(); // Compliant
        builderCalls2.Remove(builderCalls2.Length - 1, 1);

        var builderReturn = new StringBuilder(); // Compliant
        return builderReturn;

        void LocalMethod(StringBuilder local)
        {
            local.ToString();
        }
    }

    public StringBuilder GetStringBuilder() => // Compliant
        new StringBuilder();

    public void ExternalMethod(StringBuilder builder) { } // Compliant

    public string AnotherMethod()
    {
        var builder = new StringBuilder(); // FP
        return $"{builder} is ToStringed here";
    }

    public string MyProperty
    {
        get
        {
            var builder1 = new StringBuilder(); // Noncompliant
            var builder2 = new StringBuilder(); // Compliant
            return builder2.ToString();
        }
    }
}
