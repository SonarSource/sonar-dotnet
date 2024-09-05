using System;
using System.Collections.Generic;

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

class ParamsCollections
{
    private void Format(string a, params List<object> b) => Console.WriteLine("Format with List<object>");
    
    private void Format(object a, object b, object c) => Console.WriteLine("Format with three objects");
    
    public void Method()
    {
        ParamsCollections paramsCollections = new ParamsCollections();

        paramsCollections.Format("", null, null); // Noncompliant {{Review this call, which partially matches an overload without 'params'. The partial match is 'void ParamsCollections.Format(object a, object b, object c)'.}}
        paramsCollections.Format("", new List<object> { null, null }); // Compliant
        paramsCollections.Format("", [new object(), null]); // Compliant
    }
}
