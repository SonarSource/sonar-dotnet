public class PrimaryCtorClass() // Noncompliant {{Remove this primary constructor.}}
{
    public static string Concatenate(string s1, string s2)
    {
        return s1 + s2;
    }

    public static string Prop { get; set; }
}

public class PrimaryCtorClassWithParams(int i) // Compliant There is a captured parameter for the instance 
{
    public static string Concatenate(string s1, string s2)
    {
        return s1 + s2;
    }
}
