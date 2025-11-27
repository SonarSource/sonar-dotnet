class MyCompliantClass // Noncompliant
{
    public bool Equals(MyCompliantClass other)
    {
        return false;
    }
}

public sealed record C // Compliant, records implement IEquatable by design
{
    public bool Equals(C other)
    {
        return false;
    }
}

public class Sample { }

public static class ClassicExtensions // Compliant, extension methods doesn't allow you to implement an interface
{
    public static bool Equals(this Sample self, Sample other)
    {
        return false;
    }
}

public static class NewExtensions // Compliant, extension methods doesn't allow you to implement an interface
{
    extension(Sample sample)
    {
        bool Equals(Sample other)
        {
            return false;
        }
    }
}
