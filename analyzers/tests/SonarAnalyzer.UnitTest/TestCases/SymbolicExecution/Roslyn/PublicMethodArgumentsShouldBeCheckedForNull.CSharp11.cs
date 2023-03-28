public class Program
{
    private object field = null;
    private static object staticField = null;

    public void NotCompliantCases(object[] o)
    {
        o[1].ToString(); // Noncompliant {{Refactor this method to add validation of parameter 'o' before using it.}}
    }

    public void ListPattern1(object[] o)
    {
        if (o is [not null, not null])
        {
            o.ToString();       // Noncompliant - FP
        }
    }

    public void ListPattern2(object[] o)
    {
        if (o is [not null, not null])
        {
            o[1].ToString();    // Noncompliant - FP
        }
    }
}

public interface ISomeInterface
{
    public static virtual void NotCompliantCases(object o)
    {
        o.ToString(); // Noncompliant {{Refactor this method to add validation of parameter 'o' before using it.}}
    }
}

file class ClassWithFileAccessibility
{
    public void PublicWithArgs(object o)
    {
        o.ToString(); // Compliant - method is not accessible from other assemblies
    }
}

file interface IInterfaceWithFileAccessibility
{
    void PublicWithArgs(object o)
    {
        o.ToString();
    }
}

file struct StructWithFileAccessibility
{
    public void PublicWithArgs(object o)
    {
        o.ToString();
    }
}
