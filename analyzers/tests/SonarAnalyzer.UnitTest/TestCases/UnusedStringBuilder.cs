using System.Collections.Generic;
using System.Text;

public class Program
{
    public StringBuilder MyMethod(StringBuilder builder, string myString) // Compliant
    {
        StringBuilder sb1 = GetStringBuilder(); // Compliant

        var sb2 = new StringBuilder(); // Compliant
        ExternalMethod(sb2);

        var sb3 = new StringBuilder(); // Compliant
        LocalMethod(sb3);

        var sb4 = new StringBuilder(); // Compliant

        StringBuilder sb5 = new StringBuilder(); // Noncompliant {{Remove this "StringBuilder"; ".ToString()" is never called.}}
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^
        var sb6 = new StringBuilder(); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^

        StringBuilder sb7 = new StringBuilder(); // Noncompliant
        sb7.Append(sb4.ToString());

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
                            var buildernested = new StringBuilder(); // Noncompliant
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

    private StringBuilder myField = new StringBuilder(); // Compliant
}
