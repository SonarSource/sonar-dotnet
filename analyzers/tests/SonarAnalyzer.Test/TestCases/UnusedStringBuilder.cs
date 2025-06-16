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

        StringBuilder sb8 = new StringBuilder(); // Compliant
        StringBuilder sb9 = new StringBuilder(); // Noncompliant
        sb9 = sb8;

        StringBuilder sb10 = new StringBuilder(); // Noncompliant FP see https://github.com/SonarSource/sonar-dotnet/issues/6747
        sb10.ToStringAndFree();

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

        StringBuilder builderInLine1 = new StringBuilder(), builderInLine2 = new StringBuilder();
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                                          Noncompliant
//                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    Noncompliant@-1


        StringBuilder builderCfg = new StringBuilder(); // FN (requires use of cfg with significant impact on performance)
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

        StringBuilder mySbField = new StringBuilder(); // Noncompliant
        MyClass myClass = new MyClass();
        myClass.mySbField.ToString();

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
        var builder = new StringBuilder(); // Compliant
        return $"{builder} is ToStringed here";
    }

    public void Scoping()
    {
        {
            var sb = new StringBuilder(); // Noncompliant
        }
        {
            var sb = new StringBuilder();
            sb.ToString();
        }
    }

    public string CompoundAdd()
    {
        var sb = new StringBuilder();   // Compliant
        sb.Append("Lorem ipsum");
        string ret = "";
        ret += sb;
        return ret;
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

public class MyClass
{
    public StringBuilder mySbField = new StringBuilder();
}

public static class StringBuilderExtensions
{
    public static string ToStringAndFree(this StringBuilder sb) => string.Empty;
}

// https://github.com/SonarSource/sonar-dotnet/issues/7324
public class Repro_7324
{
    public string Concat_Prefix()
    {
        var sb = new StringBuilder();   // Compliant
        sb.Append("Lorem ipsum");
        var ret = "Prefix: " + sb;
        return ret;
    }

    public string Concat_Infix()
    {
        var sb = new StringBuilder();   // Compliant
        sb.Append("Lorem ipsum");
        var ret = "Prefix: " + sb + " suffix";
        return ret;
    }

    public string Concat_Suffix()
    {
        var sb = new StringBuilder();   // Compliant
        sb.Append("Lorem ipsum");
        var ret = sb + " suffix";
        return ret;
    }

    public string Concat_OutsideDeclaration()
    {
        var sb = new StringBuilder();   // Compliant
        sb.Append("Lorem ipsum");
        string ret;
        ret = "Prefix: " + sb;
        return ret;
    }
}
