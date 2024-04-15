using System.IO;
using System.Threading.Tasks;
using System;

public class C
{
    C this[int i] => null;
    public static implicit operator int(C c) => default(C);

    C ReturnMethod() => null;

    async Task OperatorPrecedence() // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/#operator-precedence
    {
        _ = ^ReturnMethod(); // Noncompliant
    }
}
