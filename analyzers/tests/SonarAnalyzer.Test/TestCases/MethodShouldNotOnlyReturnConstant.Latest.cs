using System;

namespace Tests.Diagnostics
{
    interface ISomeInterface {
        public bool IsCondition() => true; // Compliant, see: https://github.com/SonarSource/sonar-dotnet/issues/5498
    }
    public interface IFoo
    {
        static abstract int GetValue();

        static virtual int GetAnotherValue() { return 42; } // Compliant - interface methods are ignored
    }

    public class Foo : IFoo
    {
        public static int GetValue() // Compliant - implements interface so cannot get rid of the method
        {
            return 42;
        }

        public string Get() => "hello"; // Noncompliant

        public string Get_Raw() => """
        "Forty-two", said Deep Thought, with infinite majesty and calm.
        """;
        // Noncompliant@-3

        // ref: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#utf-8-string-literals
        public ReadOnlySpan<byte> Get_Utf_8() => "hello"u8; // Compliant, utf-8 strings are runtime constants (represented as ReadOnlySpan<byte>)
    }
}

public static class  Extensions
{
    extension (string s)
    {
        public string NewCompliant() => s + "!";
        public string NewNonCompliant() => "!"; // Noncompliant
        public int this[int index] => 42; // Compliant, only methods are inspected
    }
}

namespace CSharp15
{
    public closed class ClosedBase
    {
        public int Value() => 42; // Noncompliant
    }

    public class NewNumericTypes
    {
        public System.Numerics.Decimal64 Max() => System.Numerics.Decimal64.MaxValue; // Compliant, no literal form for the new numeric types
        public System.Numerics.BFloat16 Zero() => default; // Compliant
    }
}
