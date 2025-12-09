using System;

public class StaticLocalFunctions
{

    public void Method()
    {
        static string LocalFunction(int x)
//                    ^^^^^^^^^^^^^ Secondary
//                    ^^^^^^^^^^^^^ Secondary@-1
        {
            x += 42;
            return x.ToString();
        }

        static string LocalFunctionCopy(int x) // Noncompliant
        {
            x += 42;
            return x.ToString();
        }
    }

    public string MethodWhichCopiesLocalFunction(int x) // Noncompliant
    {
        x += 42;
        return x.ToString();
    }
}

// https://sonarsource.atlassian.net/browse/NET-348
class Repro_348
{
    T Method1<T>() => default;  // FN
    T Method2<T>() => default;
}

public record struct Sample
{
    public void Method1()
//              ^^^^^^^ Secondary
//              ^^^^^^^ Secondary@-1
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method2() // Noncompliant
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method3() // Noncompliant
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method4()
    {
        Console.WriteLine("Result: 0");
    }

    public string Method5()
    {
        return "foo";
    }

    public string Method6() =>
        "foo";
}

public record struct SamplePositional(string Value)
{
    public void Method1() // Secondary
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method2() // Noncompliant
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }
}

interface SomeInterface
{
    void Foo1() // Secondary
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    void Foo2() // Noncompliant
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }
}

public static class TypeConstraints
{
    public static int Use<T>(T? value) where T : struct => 1;

    public static int Use<T>(T? value) where T : class => 2;

    public static void First<T>(T? value) where T : struct
    {
        var x = Use(value);
        Console.WriteLine(x);
    }

    public static void Second<T>(T? value) where T : class  // Compliant, method looks the same but different overloads are called due to the type constraints used.
    {
        var x = Use(value);
        Console.WriteLine(x);
    }

    public static bool Compare1<T>(T? value1, T value2) // Secondary [Compare]
    {
        Console.WriteLine(value1);
        Console.WriteLine(value2);
        return true;
    }

    public static bool Compare2<T>(T? value1, T value2)  // Noncompliant [Compare]
    {
        Console.WriteLine(value1);
        Console.WriteLine(value2);
        return true;
    }

    public static bool Compare3<T>(T? value1, T value2) where T : System.IComparable // Compliant. Parameter type constraints don't match
    {
        Console.WriteLine(value1);
        Console.WriteLine(value2);
        return true;
    }

    public static bool Equal1<T>(T t1, T t2) where T : System.IEquatable<T>  // Secondary [Equal]
    {
        Console.WriteLine(t1);
        Console.WriteLine(t2);
        return true;
    }

    public static bool Equal2<T>(T t1, T t2) where T : System.IEquatable<T> // Noncompliant [Equal]
    {
        Console.WriteLine(t1);
        Console.WriteLine(t2);
        return true;
    }

    public static bool Equal3<T>(T t1, T t2) where T : System.IEquatable<int> // Compliant. The type constraint is different
    {
        Console.WriteLine(t1);
        Console.WriteLine(t2);
        return true;
    }

    public static bool Equal4<T>(T t1, T t2) where T : System.IComparable<T> // Compliant. The type constraint is different
    {
        Console.WriteLine(t1);
        Console.WriteLine(t2);
        return true;
    }
}

public class TypeConstraintsOnGenericClass<TClass>
{
    public void ConstraintByTClass1<TMethod>() where TMethod : TClass // Secondary [ConstraintByTClass]
    {
        Console.WriteLine("a");
        Console.WriteLine("b");
        Console.WriteLine("c");
    }

    public void ConstraintByTClass2<TMethod>() where TMethod : TClass // Noncompliant [ConstraintByTClass]
    {
        Console.WriteLine("a");
        Console.WriteLine("b");
        Console.WriteLine("c");
    }

    public void ConstraintByTClass3<TMethod>() // Compliant
    {
        Console.WriteLine("a");
        Console.WriteLine("b");
        Console.WriteLine("c");
    }

    public void ConstraintByTClass4<TMethod>() where TMethod : IEquatable<TClass> // Compliant
    {
        Console.WriteLine("a");
        Console.WriteLine("b");
        Console.WriteLine("c");
    }

    public void ConstraintByTClass5<TMethod>() where TMethod : struct, TClass // Compliant
    {
        Console.WriteLine("a");
        Console.WriteLine("b");
        Console.WriteLine("c");
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9654
public class Repro_9654
{
    public record Foo(string A);
    public record Bar(string A);

    private static Foo SameBodyDifferentReturnTypeImplicit1()
    {
        string s = "A";
        return new(s);
    }

    private static Bar SameBodyDifferentReturnTypeImplicit2()       // Compliant - different return type
    {
        string s = "A";
        return new(s);
    }

    private static Foo SameBodyDifferentReturnTypeExplicit1()
    {
        Console.WriteLine("Test");
        return new Foo("A");
    }

    private static Bar SameBodyDifferentReturnTypeExplicit2()
    {
        Console.WriteLine("Test");
        return new Bar("A");
    }

    private static int SameBodyDifferentReturnTypeLiteral1()
    {
        Console.WriteLine("Test");
        return 42;
    }

    private static double SameBodyDifferentReturnTypeLiteral2()     // Compliant - different return type
    {
        Console.WriteLine("Test");
        return 42;
    }

    private static string SameReturnTypeWithDifferentName1()        // Secondary [SameReturnTypeWithDifferentName]
    {
        Console.WriteLine("Test");
        return "A";
    }

    private static System.String SameReturnTypeWithDifferentName2() // Noncompliant [SameReturnTypeWithDifferentName]
    {
        Console.WriteLine("Test");
        return "A";
    }

    private static UnkownType1 SameImplementationWithUnknownReturnType1() // Error[CS0246]
    {
        Console.WriteLine("Test");
        return "A";
    }

    private static UnkownType2 SameImplementationWithUnknownReturnType2() // Error[CS0246]
    {
        Console.WriteLine("Test");
        return "A";
    }
}

namespace Tests.Diagnostics
{
    interface IInterface
    {
        static virtual void First()
//                          ^^^^^ Secondary
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        static virtual void Second() // Noncompliant {{Update this method so that its implementation is not identical to 'First'.}}
//                          ^^^^^^
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        static virtual void Different()
        {
            string s = "this is a different method";
            Console.WriteLine("Result: {0}", s);
        }
    }
}

public class ExtensionTest
{
    public void MethodInBase()
    {
        Console.WriteLine("One");
        Console.WriteLine("Two");
    }
}

public static class NewExtensions
{
    extension(ExtensionTest sample)
    {
        public void IdenticalToBaseMethod() // FN https://sonarsource.atlassian.net/browse/NET-2772
        {
            Console.WriteLine("One");
            Console.WriteLine("Two");
        }

        public void MethodInExtension() // FN https://sonarsource.atlassian.net/browse/NET-2745
        {
            Console.WriteLine("One");
            Console.WriteLine("Two");
            Console.WriteLine("Three");
        }

        public void MethodInExtension2() // FN https://sonarsource.atlassian.net/browse/NET-2745
        {
            Console.WriteLine("One");
            Console.WriteLine("Two");
            Console.WriteLine("Three");
        }
    }
}
