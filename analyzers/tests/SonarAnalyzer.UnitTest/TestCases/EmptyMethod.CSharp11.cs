
interface IMyInterface
{
    static virtual void StaticVirtualMethod() // Compliant (empty virtual method)
    {

    }

    static abstract void StaticAbstractMethod(); 
}

class MyClass : IMyInterface
{
    public static void StaticVirtualMethod() { } // Noncompliant
    
    public static void StaticAbstractMethod() { } // Noncompliant
}
