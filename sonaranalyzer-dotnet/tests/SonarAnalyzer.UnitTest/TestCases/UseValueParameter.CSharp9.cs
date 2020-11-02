record UseValueParameter
{
    int count;

    public int Count
    {
        get { return count; }
        init { count = 3; } // Compliant FN
    }

    public string FirstName { get; init; } = "Foo";

    public string LastName { get => string.Empty; init { } } // Compliant FN

    public int this[int i]
    {
        get => 0;
        init // Compliant FN
        {
            var x = 1;
        }
    }
}

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
