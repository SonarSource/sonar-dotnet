using System.Collections.Generic;
using System.Text;

public class Program
{
    public StringBuilder MyMethod(StringBuilder builder) // Compliant
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
        builderCalls.Remove(builderCalls.Length - 1, 1);
        builderCalls.Insert(builderCalls.Length, "\r\n");
        builderCalls.Clear();
        builderCalls.GetType();

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
