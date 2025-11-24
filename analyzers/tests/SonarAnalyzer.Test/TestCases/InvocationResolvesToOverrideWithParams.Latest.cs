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

public class SomeClass
{
    void ClassMethod<T>() where T : IMyInterface
    {
        T.Test(42, null); // Noncompliant
    }

    private protected int PrivateProtectedOverload(object a, string b) => 42;
    public int PrivateProtectedOverload(string a, params string[] bs) => 42;

    public void Method()
    {
        PrivateProtectedOverload("s1");                  // Compliant
        PrivateProtectedOverload("s1", "s2");            // Noncompliant
        PrivateProtectedOverload(null, "s2");            // Noncompliant
        PrivateProtectedOverload(null, new[] { "s2" });  // Compliant
        PrivateProtectedOverload("42", "s1", "s2");      // Compliant
    }
}

public class OtherClass
{
    public void CheckAccessibilityInMethod(SomeClass arg)
    {
        arg.PrivateProtectedOverload("s1");                  // Compliant
        arg.PrivateProtectedOverload("s1", "s2");            // Compliant
        arg.PrivateProtectedOverload(null, "s2");            // Compliant
        arg.PrivateProtectedOverload(null, new[] { "s2" });  // Compliant
        arg.PrivateProtectedOverload("42", "s1", "s2");      // Compliant
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

public class StaticLocalFunctions
{
    public static void Test(int foo, params object[] p)
    {
        Console.WriteLine("test1");
    }

    public static void Test(double foo, object p1)
    {
        Console.WriteLine("test2");
    }

    public void Method()
    {
        static void Call()
        {
            Test(42, null); // Noncompliant {{Review this call, which partially matches an overload without 'params'. The partial match is 'void StaticLocalFunctions.Test(double foo, object p1)'.}}
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8522
class Repro_8522
{
    T Get<T>(params string[] key) => default;
    string Get(string key) => default;

    T GetBothHaveGenerics<T>(params int[] ints) => default;
    T GetBothHaveGenerics<T>(int anInt) => default;

    T GenericsWhereOneHasObjectParam<T>(params int[] ints) => default;
    T GenericsWhereOneHasObjectParam<T>(object anInt) => default;

    void Test()
    {
        Get<string>("text");                          // Compliant
        GetBothHaveGenerics<string>(1);               // Compliant, when both methods are generic it seems to resolve correctly to the T GetBothHaveGenerics<T>(int anInt).
        GenericsWhereOneHasObjectParam<string>(1);    // Noncompliant
    }
}

public static class Extensions
{
    public static void Method(Exception ex)
    {
        ex.InstanceExtension("s1");             // Compliant
        ex.InstanceExtension("s1", "s2");       // Noncompliant
        Exception.StaticExtension("s1");        // Compliant
        Exception.StaticExtension("s1", "s2");  // Noncompliant
    }

    extension(Exception ex)
    {
        public int InstanceExtension(object a, string b) => 42;
        public int InstanceExtension(string a, params string[] bs) => 42;

        public static int StaticExtension(object a, string b) => 42;
        public static int StaticExtension(string a, params string[] bs) => 42;
    }
}
