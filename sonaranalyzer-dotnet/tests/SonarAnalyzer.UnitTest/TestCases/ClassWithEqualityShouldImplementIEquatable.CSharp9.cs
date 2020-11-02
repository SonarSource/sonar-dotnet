var x = 1; // Top level statement

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

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
