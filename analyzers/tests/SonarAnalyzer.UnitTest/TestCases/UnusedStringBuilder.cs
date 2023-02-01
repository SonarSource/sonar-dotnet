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

        (StringBuilder, StringBuilder) builder9 = (new StringBuilder(), new StringBuilder()); // FN

        StringBuilder builder10 = new StringBuilder(); // FN
        if (false)
        {
            builder10.ToString();
        }

        StringBuilder builder11 = new StringBuilder(); // Noncompliant
        builder11.Append("Append");
        builder11.AppendLine("AppendLine");
        builder11.Replace("\r\n", "\n");
        builder11.Remove(builder11.Length - 1, 1);
        builder11.Insert(builder11.Length, "\r\n");
        builder11.Clear();
        builder11.GetType();

        var builder12 = new StringBuilder(); // Compliant
        return builder12;

        void LocalMethod(StringBuilder b)
        {
            b.ToString();
        }
    }

    public StringBuilder GetStringBuilder() => // Compliant
        new StringBuilder();

    public void ExternalMethod(StringBuilder builder) { } // Compliant

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
