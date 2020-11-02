using System;

int MyMethod() { return 42; } // Compliant FN
int MyMethod2() { return 42; }

MyMethod();
var i = MyMethod2();
Action<int> a = (x) => MyMethod();
SomeGenericMethod<object>();
var o = SomeGenericMethod2<object>();

T SomeGenericMethod2<T>() where T : class { return null; }
T SomeGenericMethod<T>() where T : class { return null; } // Compliant

new R().MyMethod();
i = new R().MyMethod2();

int NeverUsed(int neverUsedVal) // FN - the returned value is not used
{
    return neverUsedVal;
}

record R
{
    public int MyMethod() { return 42; } // Compliant
    public int MyMethod2() { return 42; }
}
