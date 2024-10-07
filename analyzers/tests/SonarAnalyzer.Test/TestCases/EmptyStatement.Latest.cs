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

public partial class PartialProperty
{
    private partial int Property_01 { set; }
}

public partial class PartialProperty
{
    private partial int Property_01 { set {; } } // Noncompliant
}

