using System;
using System.Threading.Tasks;

int MyMethod() { return 42; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
int MyMethod2() { return 42; }

async Task MyAsyncMethod() { return; }

MyMethod();
MyAsyncMethod();
var i = MyMethod2();
Action<int> a = (x) => MyMethod();
SomeGenericMethod<object>();
var o = SomeGenericMethod2<object>();

T SomeGenericMethod2<T>() where T : class { return null; }
T SomeGenericMethod<T>() where T : class { return null; } // Compliant

new RecordStruct().MyMethod();
i = new RecordStruct().MyMethod2();

new PositionalRecordStruct(42).MyMethod();
i = new PositionalRecordStruct(42).MyMethod2();

int NeverUsed(int neverUsedVal) // There should be at least an invocation in order to raise the issue.
{
    return neverUsedVal;
}

record struct RecordStruct
{
    public int MyMethod() { return 42; } // Compliant
    public int MyMethod2() { return 42; }

    public void Test()
    {
        SomeGenericMethod<object>();

        SomeGenericMethod2<object>();
        var o = SomeGenericMethod2<object>();
    }

    private T SomeGenericMethod<T>() where T : class { return null; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
    private T SomeGenericMethod2<T>() where T : class { return null; }
}

record struct PositionalRecordStruct(int SomeProperty)
{
    public int MyMethod() { return 42; } // Compliant
    public int MyMethod2() { return 42; }

    public void Test()
    {
        SomeGenericMethod<object>();

        SomeGenericMethod2<object>();
        var o = SomeGenericMethod2<object>();
    }

    private T SomeGenericMethod<T>() where T : class { return null; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
    private T SomeGenericMethod2<T>() where T : class { return null; }
}
