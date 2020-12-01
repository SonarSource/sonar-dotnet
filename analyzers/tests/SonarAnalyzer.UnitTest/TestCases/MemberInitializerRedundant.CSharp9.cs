using System.Runtime.CompilerServices;

class Person
{
    static int minAge = 0;
    static int maxAge = 42; // FN {{Remove the member initializer, module initializer sets an initial value for the member.}}

    [ModuleInitializer]
    internal static void Initialize()
    {
        maxAge = 42;
    }
}
