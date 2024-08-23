using System;

void Method1()// Secondary [0]
              // Secondary@-1 [1]
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method2() // Noncompliant [0] {{Update this method so that its implementation is not identical to 'Method1'.}}
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method3() // Noncompliant [1] {{Update this method so that its implementation is not identical to 'Method1'.}}
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method4()
{
    Console.WriteLine("Result: 0");
}

public record Sample
{
    public void Method1() // Secondary [2]
                          // Secondary@-1 [3]
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method2() // Noncompliant [2] {{Update this method so that its implementation is not identical to 'Method1'.}}
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method3() // Noncompliant [3] {{Update this method so that its implementation is not identical to 'Method1'.}}
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

public record SamplePositional(string Value)
{
    public void Method1() // Secondary [4]
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method2() // Noncompliant [4] {{Update this method so that its implementation is not identical to 'Method1'.}}
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

    private static Foo Test1() // Secondary [ReturnDifferentType]
    {
        string s = "A";
        return new(s);
    }

    private static Bar Test2() // Noncompliant [ReturnDifferentType] - FP
    {
        string s = "A";
        return new(s);
    }

    private static Foo Test3()
    {
        Console.WriteLine("Test");
        _ = Test1();
        return new Foo("A");
    }

    private static Bar Test4()
    {
        Console.WriteLine("Test");
        _ = Test1();
        return new Bar("A");
    }
}
