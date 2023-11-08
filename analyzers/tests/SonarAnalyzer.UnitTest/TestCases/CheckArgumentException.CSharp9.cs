using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    record Program
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
