using System;

namespace Tests.Diagnostics
{
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
