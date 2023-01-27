using System;

interface IMyInterface
{
    static virtual void Test(int foo, params object[] p) => Console.WriteLine("Test1");

    static virtual void Test(double foo, object p1) => Console.WriteLine("Test2");

    static virtual void InterfaceStaticAbstractMethod<T>() where T : IMyInterface
    {
        T.Test(42, null); // Noncompliant {{Review this call, which partially matches an overload without 'params'. The partial match is 'void IMyInterface.Test(double foo, object p1)'.}}
    }
}

class MyClass
{
    void ClassMethod<T>() where T : IMyInterface
    {
        T.Test(42, null); // Noncompliant
    }
}
