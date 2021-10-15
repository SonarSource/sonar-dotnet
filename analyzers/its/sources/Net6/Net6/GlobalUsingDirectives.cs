global using System.Runtime.CompilerServices;

namespace Net6;

public class ClassThatDoesNotUseAnythingFromCompilerServicesNamespace
{
    public void Example()
    {
        Console.WriteLine("The namespace is used in the CallerArgumentExpression file.");
    }
}
