using System;

interface IMyInterface
{
    static virtual void StaticVirtualMethod()
    {
        ; // Noncompliant
    }

    static abstract void StaticAbstractMethod();
}

class MyClass : IMyInterface
{
    public static void StaticVirtualMethod()
    {
        ; // Noncompliant
    }

    public static void StaticAbstractMethod()
    {
        ; // Noncompliant
    }
}
