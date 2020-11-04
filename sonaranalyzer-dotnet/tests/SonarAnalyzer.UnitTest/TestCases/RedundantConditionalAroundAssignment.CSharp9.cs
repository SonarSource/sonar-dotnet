int x = 5;
int y = 6;

y = y switch
{
    not 5 => 5 // Compliant - FN
};

y = y switch
{
    5 => 5 // Compliant - FN
};

record Record
{
    string x;

    string NoncompliantProperty
    {
        init
        {
            if (x != value) // Noncompliant
            {
                x = value;
            }
        }
    }

    string CompliantProperty
    {
        init { x = value; }
    }
}

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
