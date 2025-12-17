using System;
using System.Collections.Generic;

record CSharp9
{
    void Foo1(int a)
    {
        throw new ArgumentNullException("foo"); // Noncompliant {{The parameter name 'foo' is not declared in the argument list.}}
    }

    void Foo3(int a)
    {
        ArgumentException exception = new ("a", "foo"); // Noncompliant
        throw exception;
    }

    void Foo4(int a)
    {
        Action<int, int> res =
            (_, _) =>
            {
                throw new ArgumentNullException("_");
                // https://github.com/SonarSource/sonar-dotnet/issues/8319
                throw new ArgumentNullException("a"); // Noncompliant FP - we are just looking at most direct parent definition
            };
    }

    public int Foo9
    {
        init
        {
            throw new ArgumentNullException("value");
            throw new ArgumentNullException("foo");   // Noncompliant
            throw new ArgumentNullException("Foo9");
        }
    }

    public string this[int a, int b]
    {
        init
        {
            throw new ArgumentNullException("a");
            throw new ArgumentNullException("value");
        }
    }

    void ComplexCase(int a)
    {
        Action<int> simple = b =>
            {
                Action<int, int> parenthesized = (_, _) =>
                {
                    throw new ArgumentNullException("a"); // Noncompliant
                };
            };
    }
}

class ReproForIssue2488
{
    private string input;
    public string Response
    {
        get => "";
        init => this.input = value ?? throw new ArgumentNullException(nameof(value));
    }
    public string Request
    {
        get => this.input;
        init => this.input = value ?? throw new ArgumentNullException(nameof(this.Request));
    }

    public string Request2
    {
        get => this.input;
        init => this.input = value ?? throw new ArgumentNullException(nameof(Request2));
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4423
public class Repro_4423
{
    public void InsideLocalFunction_Static(string methodArg)
    {
        Something(null);

        static void Something(string localArg)
        {
            throw new ArgumentNullException(nameof(localArg));   // Compliant
            throw new ArgumentNullException(nameof(methodArg));  // Noncompliant, this method even doesn't see methodArg value
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/5094
public record Data(string Code, string Name)
{
    public string Code { get; } = Code ?? throw new ArgumentNullException(nameof(Code)); // Compliant - Code is a record parameter
    public string GetName()
    {
        return Name ?? throw new ArgumentNullException(nameof(Name)); // Compliant - Name is a record parameter
    }

    public string GetNameLambda() => Name ?? throw new ArgumentNullException(nameof(Name)); // Compliant - Name is a record parameter
}

public class TestClass
{
    public static string ClassProperty { get; }

    public record TestRecord()
    {
        public string RecordProperty { get; } = ClassProperty ?? throw new ArgumentNullException(nameof(ClassProperty)); // Noncompliant
    }
}

public record R(string Prop)
{
    public class TestClass
    {
        public static string ClassProperty { get; }

        public string RecordProperty { get; } = ClassProperty ?? throw new ArgumentNullException(nameof(Prop)); // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/5226
public class Repro_5226
{
    public void OnePositionalOneNamedCompliant(int a)
    {
        throw new ArgumentOutOfRangeException(nameof(a), message: "Sample message");
    }

    public void OnePositionalOneNamedNonCompliant(int a)
    {
        throw new ArgumentOutOfRangeException("randomString", message: nameof(a)); // Noncompliant
    }

    public void ShuffledPositions1(int a)
    {
        throw new ArgumentOutOfRangeException("randomString", actualValue: "", nameof(a)); // Noncompliant
    }

    public void ShuffledPositions2(int a)
    {
        throw new ArgumentOutOfRangeException("randomString", message: nameof(a), actualValue: ""); // Noncompliant
    }

    public void ShuffledPositions3(int a)
    {
        throw new ArgumentOutOfRangeException(nameof(a), message: "randomString", actualValue: "");
    }
}

class CSharp10
{
    public const string Part1 = "One";
    public const string Part2 = "Two";
    public const string ParamName = $"{Part1}{Part2}";
    public const string WrongParamName = $"{Part2}{Part1}";

    void Foo1(int OneTwo)
    {
        throw new ArgumentNullException(ParamName); // Compliant
    }

    void Foo2(int OneTwo)
    {
        throw new ArgumentNullException(WrongParamName); // Noncompliant
    }
}


// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7714
public class PrimaryConstructor(string s, string q)
{
    private readonly string _s = s ?? throw new ArgumentNullException(nameof(s)); // Compliant: s is a class parameter

    public string GetQ()
    {
        ArgumentNullException.ThrowIfNull(s, "something"); // FN
        ArgumentNullException.ThrowIfNull(s, nameof(q)); // Compliant
        return q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
    }

    public string GetQLambda() => q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
}

public struct PrimaryConstructorStruct(string s, string q)
{
    private readonly string _s = s ?? throw new ArgumentNullException(nameof(s)); // Compliant: s is a class parameter

    public string GetQ()
    {
        return q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
    }

    public string GetQLambda() => q ?? throw new ArgumentNullException(nameof(q)); // Compliant: q is a class parameter
}

public class PrimaryConstructor2(string Prop)
{
    public class TestClass
    {
        public static string B { get; }

        // https://github.com/SonarSource/sonar-dotnet/issues/8319
        public string A { get; } = B ?? throw new ArgumentNullException(nameof(Prop)); // Noncompliant FP: we are checking only the first parent (TestClass) 
    }
}

public static class Extensions
{
    extension(IEnumerable<string> source)
    {
        public void InExtension()
        {
            if (source == null)
                throw new ArgumentNullException("source");  // Compliant
        }
    }

    public static void Compliant(this IEnumerable<string> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");      // Compliant
    }
}
