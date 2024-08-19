using System.Runtime.CompilerServices;

interface MyInterface
{
    static abstract void Method(string callerFilePath, string other);
}

class DerivedClass : MyInterface
{
    public static void Method([CallerFilePath] string callerFilePath = "", string other = "") // Compliant, method from interface
    {
    }
}
