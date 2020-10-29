record UseValueParameter
{
    int count;

    public int Count
    {
        get { return count; }
        init { count = 3; } // Noncompliant {{Use the 'value' parameter in this property set accessor declaration.}}
                            //      ^^^^
    }

    public string FirstName { get; init; } = "Foo";

    public string LastName { get => string.Empty; init { } } // Noncompliant

    public int this[int i]
    {
        get => 0;
        init // Noncompliant
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
