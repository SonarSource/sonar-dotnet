using System;

int MyMethod() { return 42; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
int MyMethod2() { return 42; }

MyMethod();
var i = MyMethod2();
Action<int> a = (x) => MyMethod();
SomeGenericMethod<object>();
var o = SomeGenericMethod2<object>();

T SomeGenericMethod2<T>() where T : class { return null; }
T SomeGenericMethod<T>() where T : class { return null; } // Compliant

new Record().MyMethod();
i = new Record().MyMethod2();

new PositionalRecord(42).MyMethod();
i = new PositionalRecord(42).MyMethod2();

int NeverUsed(int neverUsedVal) // There should be at least an invocation in order to raise the issue.
{
    return neverUsedVal;
}

record Record
{
    public int MyMethod() { return 42; } // Compliant
    public int MyMethod2() { return 42; }

    public void Test()
    {
        SomeGenericMethod<object>();

        SomeGenericMethod2<object>();
        var o = SomeGenericMethod2<object>();
    }

    private T SomeGenericMethod<T>() where T : class { return null; } // Noncompliant
    private T SomeGenericMethod2<T>() where T : class { return null; }
}

record PositionalRecord(int SomeProperty)
{
    public int MyMethod() { return 42; } // Compliant
    public int MyMethod2() { return 42; }

    public void Test()
    {
        SomeGenericMethod<object>();

        SomeGenericMethod2<object>();
        var o = SomeGenericMethod2<object>();
    }

    private T SomeGenericMethod<T>() where T : class { return null; } // Noncompliant
    private T SomeGenericMethod2<T>() where T : class { return null; }
}

interface InterfaceWithDefaultImplementation
{
    int SomeMethod() // Compliant, part of the interface
    {
        return 42;
    }
}

class SomeClass : InterfaceWithDefaultImplementation
{
    public void Test()
    {
        ((InterfaceWithDefaultImplementation)this).SomeMethod();
    }
}
